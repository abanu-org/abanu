// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lonos.Kernel.Core.Boot;
using Lonos.Kernel.Core.Collections;
using Lonos.Kernel.Core.Diagnostics;
using Lonos.Kernel.Core.Elf;
using Lonos.Kernel.Core.MemoryManagement;
using Lonos.Kernel.Core.MemoryManagement.PageAllocators;
using Lonos.Kernel.Core.PageManagement;
using Lonos.Kernel.Core.Scheduling;
using Mosa.Runtime.x86;

namespace Lonos.Kernel.Core.Processes
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

            System = CreateEmptyProcess(new ProcessCreateOptions());
            System.Path = "/system/main";

            Scheduler.Setup(followupTask);
            Scheduler.Start();

            Panic.Error("Should never get here");
        }

        public static Process CreateEmptyProcess(ProcessCreateOptions options)
        {
            var proc = new Process();
            proc.ProcessID = (uint)Interlocked.Increment(ref NextCreateProcessID);
            lock (ProcessList)
                ProcessList.Add(proc);
            proc.User = options.User;
            proc.PageTable = PageTable.KernelTable;
            proc.Service = new Service(proc);
            return proc;
        }

        public static KList<Process> ProcessList;

        public static Process GetProcess(uint processID)
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

        private static unsafe Process StartProcessFromElf(ElfHelper elf, string path, uint argumentBufferSize = 0)
        {
            var proc = CreateEmptyProcess(new ProcessCreateOptions() { User = true });
            KernelMessage.WriteLine("Create proc: {0}, PID: {1}", path, proc.ProcessID);
            proc.Path = path;
            proc.PageTable = PageTable.CreateInstance();
            var allocator = new UserInitialPageAllocator();
            allocator.Setup(new MemoryRegion(500 * 1024 * 1024, 20 * 1024 * 1014), AddressSpaceKind.Virtual);
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
                    continue;

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

        private static unsafe Addr GetEntryPointFromElf(ElfHelper elf)
        {
            var symName = "Lonos.Kernel.Program::Main()"; // TODO
            var sym = elf.GetSymbol(symName);
            if (sym == (ElfSymbol*)0)
                return Addr.Zero;
            return sym->Value;
        }

    }

}
