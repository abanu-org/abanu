using lonos.Kernel.Core.Elf;
using lonos.Kernel.Core.Interrupts;
using lonos.Kernel.Core.MemoryManagement;
using lonos.Kernel.Core.PageManagement;
using lonos.Kernel.Core.Processes;
using System;

namespace lonos.Kernel.Core.Scheduling
{
    public unsafe class Thread
    {
        public ThreadStatus Status = ThreadStatus.Empty;
        public Addr StackBottom;
        public Addr StackTop;
        //public IntPtr StackStatePointer;
        public IDTTaskStack* StackState;
        public Addr kernelStack = null;
        public Addr kernelStackBottom = null;
        public USize kernelStackSize = null;
        public uint Ticks;
        public bool User;
        public Process Process;
        public uint DataSelector;
        public uint ThreadID;
        public bool Debug;
        public string DebugName;

        public Thread ChildThread;
        public Thread ParentThread;

        public void FreeMemory()
        {
            RawVirtualFrameAllocator.FreeRawVirtalMemoryPages(StackTop);
            if (User)
                RawVirtualFrameAllocator.FreeRawVirtalMemoryPages(StackState);
        }

        public void Start()
        {
            Status = ThreadStatus.ScheduleForStart;
        }
    }

    public class KernelThread : Thread
    {
    }

    public class UserThread : Thread
    {
    }

    public class Service
    {

        public Process Process;

        public Service(Process proc)
        {
            this.Process = proc;
        }

        // Methods is always called within Interrupt with Interrupt disabled
        public void SwitchToThreadMethod()
        {
            var elf = KernelElf.FromSectionName(Process.Path);
            var methodAddr = GetEntryPointFromElf(elf);
            var cThread = Scheduler.GetCurrentThread();
            var th = Scheduler.CreateThread(Process, new ThreadStartOptions(methodAddr));

            // Connect Threads
            cThread.ChildThread = th;
            cThread.Status = ThreadStatus.Waiting;
            th.ParentThread = cThread;

            th.Start();
            Scheduler.SwitchToThread(th.ThreadID);
        }

        private unsafe static Addr GetEntryPointFromElf(ElfHelper elf)
        {
            var symName = "System.Void lonos.Kernel.Program::Func1()"; // TODO
            var sym = elf.GetSymbol(symName);
            if (sym == (ElfSymbol*)0)
                return Addr.Zero;
            return sym->Value;
        }

    }

}
