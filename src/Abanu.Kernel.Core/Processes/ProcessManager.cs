// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

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

        public static Process Idle;
        public static Process System;

        private static int NextCreateProcessID;

        /// <summary>
        /// Setup the <see cref="Scheduler"/> and <see cref="ProcessManager"/>
        /// </summary>
        /// <param name="followupTask">After enabling Scheduling, the Kernel will continue with this task.</param>
        public static void Setup(ThreadStart followupTask)
        {
            ProcessList = new KList<Process>();

            // Create idle task
            Idle = CreateEmptyProcess(new ProcessCreateOptions());
            Idle.Path = "/system/idle";
            Idle.RunState = ProcessRunState.Running;

            // Create system task
            System = CreateEmptyProcess(new ProcessCreateOptions());
            System.Path = "/system/main";
            System.RunState = ProcessRunState.Running;

            // Initialize scheduler
            Scheduler.Setup(followupTask);
            Scheduler.Start();

            // If we ever get here, Scheduler as not able to switch to followupTask.
            Panic.Error("Should never get here");
        }

        /// <summary>
        /// Create an empty Process structure. It will not registered or started.
        /// </summary>
        public static Process CreateEmptyProcess(ProcessCreateOptions options)
        {
            var proc = new Process();
            proc.ProcessID = Interlocked.Increment(ref NextCreateProcessID);
            lock (ProcessList)
                ProcessList.Add(proc);
            proc.User = options.User;
            proc.PageTable = PageTable.KernelTable;
            proc.Service = new ProcessService(proc);
            return proc;
        }

        /// <summary>
        /// Holds the reference of every process
        /// </summary>
        public static KList<Process> ProcessList;

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

            // TEMP: TODO: Use Program Headers!
            bool isNativeC = false;

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

                // TODO: Use Program Headers!
                if (virtAddr > 0 && virtAddr < 0x10000)
                    continue;
                if (name->Equals(".eh_frame_hdr"))
                {
                    isNativeC = true;
                    continue;
                }
                if (name->Equals(".eh_frame"))
                    continue;
                if (name->Equals(".dynamic"))
                    continue;
                if (name->Equals(".comment"))
                    continue;
                if (name->Equals(".debug_aranges"))
                    continue;
                if (name->Equals(".debug_info"))
                    continue;
                if (name->Equals(".debug_abbrev"))
                    continue;
                if (name->Equals(".debug_line"))
                    continue;
                if (name->Equals(".debug_str"))
                    continue;
                if (name->Equals(".symtab") && isNativeC)
                    continue;
                if (name->Equals(".strtab") && isNativeC)
                    continue;
                if (name->Equals(".shstrtab") && isNativeC)
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
            var entryPoint = GetMainEntryPointFromElf(elf);
            KernelMessage.WriteLine("EntryPoint: {0:X8}", entryPoint);
            var defaultDispatchEntryPoint = GetDispatchEntryPointFromElf(elf);
            if (defaultDispatchEntryPoint != Addr.Zero)
            {
                KernelMessage.WriteLine("DispatchEntryPoint: {0:X8}", defaultDispatchEntryPoint);
                proc.Service.Init(defaultDispatchEntryPoint);
            }

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

        public static Process GetProcessByID(int processID)
        {
            lock (ProcessList)
                for (var i = 0; i < ProcessList.Count; i++)
                    if (ProcessList[i].ProcessID == processID)
                        return ProcessList[i];
            return null;
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

        public static void KillProcessByID(int processID)
        {
            KernelMessage.WriteLine("Killing process ID {0}", processID);
            var proc = GetProcessByID(processID);
            if (proc == null)
                return;

            for (var i = 0; i < proc.Threads.Count; i++)
            {
                var th = proc.Threads[i];
                th.Terminate();
            }

            proc.RunState = ProcessRunState.Terminated;
        }

        private static unsafe Addr GetMainEntryPointFromElf(ElfSections elf)
        {
            var addr = GetEntryPointFromElf(elf, "Abanu.Kernel.Program::Main()");
            if (addr == Addr.Zero)
            {
                addr = elf.GetSectionHeader(".text")->Addr;
            }
            return addr;
        }

        private static unsafe Addr GetDispatchEntryPointFromElf(ElfSections elf)
        {
            return GetEntryPointFromElf(elf, "Abanu.Runtime.MessageManager::Dispatch(Abanu.Kernel.SystemMessage)");
        }

        private static unsafe Addr GetEntryPointFromElf(ElfSections elf, string symbolName)
        {
            var sym = elf.GetSymbol(symbolName);
            if (sym == (ElfSymbol*)0)
                return Addr.Zero;
            return sym->Value;
        }

        /// <summary>
        /// Dump the statistics of interest
        /// </summary>
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
