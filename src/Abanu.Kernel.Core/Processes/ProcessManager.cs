// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Abanu.Kernel.Core.Boot;
using Abanu.Kernel.Core.Collections;
using Abanu.Kernel.Core.Diagnostics;
using Abanu.Kernel.Core.Elf;
using Abanu.Kernel.Core.MemoryManagement;
using Abanu.Kernel.Core.MemoryManagement.PageAllocators;
using Abanu.Kernel.Core.PageManagement;
using Abanu.Kernel.Core.Scheduling;
using Mosa.Runtime.x86;

namespace Abanu.Kernel.Core.Processes
{

    public static class ProcessManager
    {

        private static int NextCreateProcessID;
        public static Process Idle;
        public static Process System;

        public static void Setup(ThreadStart followupTask)
        {
            ProcessList = new KList<Process>();

            Idle = CreateEmptyProcess(new ProcessCreateOptions());
            Idle.Path = "/system/idle";
            Idle.RunState = ProcessRunState.Running;

            System = CreateEmptyProcess(new ProcessCreateOptions());
            System.Path = "/system/main";
            System.RunState = ProcessRunState.Running;

            Scheduler.Setup(followupTask);
            Scheduler.Start();

            Panic.Error("Should never get here");
        }

        public static Process CreateEmptyProcess(ProcessCreateOptions options)
        {
            var proc = new Process();
            proc.ProcessID = Interlocked.Increment(ref NextCreateProcessID);
            lock (ProcessList)
                ProcessList.Add(proc);
            proc.User = options.User;
            proc.PageTable = PageTable.KernelTable;
            proc.Service = new Service(proc);
            return proc;
        }

        public static KList<Process> ProcessList;

        public static Process GetProcess(int processID)
        {
            lock (ProcessList)
                for (var i = 0; i < ProcessList.Count; i++)
                    if (ProcessList[i].ProcessID == processID)
                        return ProcessList[i];
            return null;
        }

        public static unsafe Process StartProcessFromBuffer(MemoryRegion region, uint argumentBufferSize = 0)
        {
            KernelMessage.WriteLine("StartProcessFromBuffer at {0:X8} size {1:X8}", region.Start, region.Size);
            var cs = region.Checksum();
            KernelMessage.WriteLine("CheckSum: {0:X8}", cs);

            // TODO: Copy Buffer!!
            var elf = KernelElf.FromAddress(region.Start);
            return StartProcessFromElf(elf, "memory", argumentBufferSize);
        }

        public static unsafe Process StartProcess(string path, uint argumentBufferSize = 0)
        {
            var elf = KernelElf.FromSectionName(path);
            return StartProcessFromElf(elf, path, argumentBufferSize);
        }

        private static unsafe Process StartProcessFromElf(ElfSections elf, string path, uint argumentBufferSize = 0)
        {
            var proc = CreateEmptyProcess(new ProcessCreateOptions() { User = true });
            KernelMessage.WriteLine("Create proc: {0}, PID: {1}", path, proc.ProcessID);
            proc.Path = path;
            proc.PageTable = PageTable.CreateInstance();

            var allocator = new UserInitialPageAllocator() { DebugName = "UserInitial" };
            allocator.Setup(new MemoryRegion(500 * 1024 * 1024, 60 * 1024 * 1014), AddressSpaceKind.Virtual);
            proc.UserPageAllocator = allocator;

            // Setup User PageTable
            proc.PageTableAllocAddr = VirtualPageManager.AllocatePages(
                KMath.DivCeil(proc.PageTable.InitalMemoryAllocationSize, 4096),
                new AllocatePageOptions { Pool = PageAllocationPool.Identity });
            PageTable.KernelTable.SetWritable(proc.PageTableAllocAddr, proc.PageTable.InitalMemoryAllocationSize);
            proc.PageTable.UserProcSetup(proc.PageTableAllocAddr);
            proc.PageTable.Map(proc.PageTableAllocAddr, proc.PageTableAllocAddr, proc.PageTable.InitalMemoryAllocationSize);

            proc.PageTable.MapCopy(PageTable.KernelTable, BootInfoMemoryType.KernelTextSegment);
            proc.PageTable.SetExecutable(BootInfoMemoryType.KernelTextSegment);
            proc.PageTable.MapCopy(PageTable.KernelTable, Address.InterruptControlBlock, 4096);
            proc.PageTable.MapCopy(PageTable.KernelTable, KernelMemoryMapManager.Header->Used.GetMap(BootInfoMemoryType.GDT));
            proc.PageTable.MapCopy(PageTable.KernelTable, KernelMemoryMapManager.Header->Used.GetMap(BootInfoMemoryType.IDT));
            proc.PageTable.MapCopy(PageTable.KernelTable, KernelMemoryMapManager.Header->Used.GetMap(BootInfoMemoryType.TSS));

            var tmpKernelElfHeaders = SetupElfHeader(proc, elf);

            // Setup ELF Sections
            for (uint i = 0; i < elf.SectionHeaderCount; i++)
            {
                var section = elf.GetSectionHeader(i);
                var name = elf.GeSectionName(section);

                var size = section->Size;
                var virtAddr = section->Addr;
                var srcAddr = elf.GetSectionPhysAddr(section);

                if (size == 0)
                    continue;

                if (virtAddr == Addr.Zero)
                {
                    var mem = allocator.AllocatePagesAddr(KMath.DivCeil(size, 4096));
                    tmpKernelElfHeaders[i].Addr = mem;
                    virtAddr = mem;
                }

                var sb = new StringBuffer();
                sb.Append("Map section ");
                sb.Append(name);
                sb.Append(" virt={0:X8} src={1:X8} size={2:X8}", virtAddr, srcAddr, size);
                KernelMessage.WriteLine(sb);
                //MemoryOperation.Copy4(elf.GetSectionPhysAddr(section), section->Addr, section->Size);

                // Map the Sections
                proc.PageTable.MapCopy(PageTable.KernelTable, srcAddr, virtAddr, size);
                if (name->Equals(".text"))
                    proc.PageTable.SetExecutable(virtAddr, size);

            }
            KernelMessage.WriteLine("proc sections are ready");

            // Detect Thread-Main
            var entryPoint = GetEntryPointFromElf(elf);
            KernelMessage.WriteLine("EntryPoint: {0:X8}", entryPoint);

            var mainThread = Scheduler.CreateThread(proc, new ThreadStartOptions(entryPoint)
            {
                ArgumentBufferSize = argumentBufferSize,
                AllowUserModeIOPort = true,
                DebugName = "UserProcMainThread",
            });
            KernelMessage.WriteLine("Starting {0} on Thread {1}", path, mainThread.ThreadID);
            proc.Start();

            return proc;
        }

        /// <summary>
        /// Used for app, so it can access it's own sections
        /// </summary>
        private static unsafe ElfSectionHeader* SetupElfHeader(Process proc, ElfSections elf)
        {
            var kernelAddr = VirtualPageManager.AllocatePages(1);
            var userAddr = proc.UserPageAllocator.AllocatePagesAddr(1);
            KernelMessage.WriteLine("Store User KernelSectionsInfo at {0:X8}", userAddr);
            proc.PageTable.MapCopy(PageTable.KernelTable, kernelAddr, userAddr, 4096);
            var kernelHelper = (ElfSections*)kernelAddr;
            *kernelHelper = elf;
            kernelHelper->PhyOffset = 0;
            var kernelSectionHeaderArray = (ElfSectionHeader*)(kernelAddr + sizeof(ElfSections));
            for (var i = 0; i < elf.SectionHeaderCount; i++)
                kernelSectionHeaderArray[i] = elf.SectionHeaderArray[i];
            kernelHelper->SectionHeaderArray = (ElfSectionHeader*)(userAddr + sizeof(ElfSections));
            proc.UserElfSectionsAddr = userAddr;
            return kernelSectionHeaderArray;
        }

        public static void KillProcess(int processID)
        {
            KernelMessage.WriteLine("Killing process ID {0}", processID);
            var proc = GetProcess(processID);
            if (proc == null)
                return;

            for (var i = 0; i < proc.Threads.Count; i++)
            {
                var th = proc.Threads[i];
                th.Terminate();
            }

            proc.RunState = ProcessRunState.Terminated;
        }

        private static unsafe Addr GetEntryPointFromElf(ElfSections elf)
        {
            var symName = "Abanu.Kernel.Program::Main()"; // TODO
            var sym = elf.GetSymbol(symName);
            if (sym == (ElfSymbol*)0)
                return Addr.Zero;
            return sym->Value;
        }

        public static unsafe Process GetProcessByName(NullTerminatedString* name)
        {
            for (var i = 0; i < ProcessList.Count; i++)
            {
                var proc = ProcessList[i];
                if (name->Equals(proc.Path))
                    if (proc.RunState == ProcessRunState.Running)
                        return proc;
            }
            return null;
        }

        public static unsafe void DumpStats()
        {
            for (var i = 0; i < ProcessList.Count; i++)
            {
                var proc = ProcessList[i];
                KernelMessage.WriteLine("PID {1} Name {0} State {2}", proc.Path, (uint)proc.ProcessID, (uint)proc.RunState);
            }
        }

    }

}
