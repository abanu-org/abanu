using lonos.Kernel.Core.Elf;
using lonos.Kernel.Core.Processes;

namespace lonos.Kernel.Core.Scheduling
{
    public class Service
    {

        public Process Process;

        public Service(Process proc)
        {
            this.Process = proc;
        }

        // Methods is always called within Interrupt with Interrupt disabled
        public unsafe void SwitchToThreadMethod(uint arg0)
        {
            var elf = KernelElf.FromSectionName(Process.Path);
            var methodAddr = GetEntryPointFromElf(elf);
            var th = CreateThread(methodAddr, 4);
            SwitchToThread(th);
        }

        public Thread CreateThread(uint methodAddr, uint argumentBufferSize)
        {
            return Scheduler.CreateThread(Process, new ThreadStartOptions(methodAddr) { ArgumentBufferSize = argumentBufferSize });
        }

        public unsafe void SwitchToThread(Thread th)
        {
            var cThread = Scheduler.GetCurrentThread();

            // Connect Threads
            cThread.ChildThread = th;
            cThread.Status = ThreadStatus.Waiting;
            th.ParentThread = cThread;

            //th.StackState->Stack.ECX = arg0;

            th.Start();
            Scheduler.SwitchToThread(th.ThreadID);
        }

        private unsafe static Addr GetEntryPointFromElf(ElfHelper elf)
        {
            var symName = "lonos.Kernel.Program::Func1(System.UInt32)"; // TODO
            var sym = elf.GetSymbol(symName);
            if (sym == (ElfSymbol*)0)
                return Addr.Zero;
            return sym->Value;
        }

    }

}
