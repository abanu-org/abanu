// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using Lonos.Kernel.Core.Elf;
using Lonos.Kernel.Core.Processes;
using Lonos.Kernel.Core.SysCalls;

namespace Lonos.Kernel.Core.Scheduling
{
    public class Service
    {

        public Process Process;

        public Service(Process proc)
        {
            this.Process = proc;
        }

        // Methods is always called within Interrupt with Interrupt disabled
        public unsafe void SwitchToThreadMethod(SystemMessage* args)
        {
            var elf = KernelElf.FromSectionName(Process.Path);
            var methodAddr = GetEntryPointFromElf(elf);
            var th = CreateThread(methodAddr, SystemMessage.Size);
            var argAddr = (SystemMessage*)th.GetArgumentAddr(0);
            argAddr[0] = *(args);
            SwitchToThread(th);
        }

        public Thread CreateThread(uint methodAddr, uint argumentBufferSize)
        {
            return Scheduler.CreateThread(Process, new ThreadStartOptions(methodAddr) { ArgumentBufferSize = argumentBufferSize });
        }

        public static unsafe void SwitchToThread(Thread th)
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

        private static string DispatchSymbol = "lonos.Runtime.MessageManager::Dispatch(lonos.Kernel.SystemMessage)";

        private static unsafe Addr GetEntryPointFromElf(ElfHelper elf)
        {
            var sym = elf.GetSymbol(DispatchSymbol);
            if (sym == (ElfSymbol*)0)
                return Addr.Zero;
            return sym->Value;
        }

    }

}
