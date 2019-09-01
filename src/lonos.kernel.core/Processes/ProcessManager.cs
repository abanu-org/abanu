using lonos.Kernel.Core.Boot;
using lonos.Kernel.Core.Elf;
using lonos.Kernel.Core.MemoryManagement;
using lonos.Kernel.Core.PageManagement;
using lonos.Kernel.Core.Scheduling;
using Mosa.Runtime.x86;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace lonos.Kernel.Core.Processes
{

    public static class ProcessManager
    {

        private static int NextCreateProcessID;
        public static Process Idle;
        public static Process System;

        public static void Setup(ThreadStart followupTask)
        {
            Idle = CreateEmptyProcess(new ProcessCreateOptions());
            Idle.Path = "/system/idle";

            System = CreateEmptyProcess(new ProcessCreateOptions());
            System.Path = "/system/main";

            Scheduler.Setup(followupTask);
            Scheduler.Start();

            while (true)
                Native.Hlt();
        }

        public static Process CreateEmptyProcess(ProcessCreateOptions options)
        {
            var proc = new Process();
            proc.ProcessID = (uint)Interlocked.Increment(ref NextCreateProcessID);
            proc.User = options.User;
            proc.PageTable = PageTable.KernelTable;
            return proc;
        }

        public static unsafe void StartProcess(string path)
        {
            KernelMessage.WriteLine("Create proc: {0}", path);

            var proc = CreateEmptyProcess(new ProcessCreateOptions() { User = true });
            proc.PageTable = PageTable.CreateInstance();

            var pageTableAddr = RawVirtualFrameAllocator.RequestIdentityMappedVirtalMemoryPages(KMath.DivCeil(proc.PageTable.InitalMemoryAllocationSize, 4096));
            PageTable.KernelTable.WritableBySize(pageTableAddr, proc.PageTable.InitalMemoryAllocationSize);
            proc.PageTable.UserProcSetup(pageTableAddr);

            proc.PageTable.MapCopy(PageTable.KernelTable, BootInfoMemoryType.KernelTextSegment);
            proc.PageTable.MapCopy(PageTable.KernelTable, Address.InterruptControlBlock, 4096);

            var elf = KernelElf.FromSectionName(path);
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

                proc.PageTable.Map(virtAddr, srcAddr, size);
                proc.PageTable.SetExecutableForRegion(virtAddr, size);
                //PageTable.KernelTable.Map(virtAddr, srcAddr, size);
            }
            KernelMessage.WriteLine("proc sections are ready");

            var entryPoint = GetEntryPointFromElf(elf);
            KernelMessage.WriteLine("EntryPoint: {0:X8}", entryPoint);

            var mainThread = Scheduler.CreateThread(proc, new ThreadStartOptions(entryPoint) { AllowUserModeIOPort = true, Debug = true });
            proc.Start();
        }

        private unsafe static Addr GetEntryPointFromElf(ElfHelper elf)
        {
            var symName = "System.Void lonos.Kernel.Program::Main()"; // TODO
            var sym = elf.GetSymbol(symName);
            if (sym == (ElfSymbol*)0)
                return Addr.Zero;
            return sym->Value;
        }

    }

}
