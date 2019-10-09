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
        public ServiceStatus Status;

        private Addr MethodAddr;

        public Service(Process proc)
        {
            Process = proc;
            Status = ServiceStatus.NotInizialized;
        }

        internal void Init()
        {
            bool success;
            var elf = KernelElf.FromSectionName(Process.Path, out success);
            if (success)
            {
                MethodAddr = GetEntryPointFromElf(elf);
            }
        }

        // Methods is always called within Interrupt with Interrupt disabled

        // TODO: Code duplication! Both SwitchToThreadMethod are very similar.

        public unsafe void SwitchToThreadMethod(SysCallContext* context, SystemMessage* args)
        {
            var th = CreateThread(MethodAddr, SystemMessage.Size);
            th.DebugSystemMessage = *args;
            var argAddr = (SystemMessage*)th.GetArgumentAddr(0);
            argAddr[0] = *args;
            SwitchToThread(context, th);
        }

        public Thread CreateThread(uint methodAddr, uint argumentBufferSize)
        {
            return Scheduler.CreateThread(Process, new ThreadStartOptions(methodAddr) { ArgumentBufferSize = argumentBufferSize, DebugName = "ServiceCall" });
        }

        public static unsafe void SwitchToThread(SysCallContext* context, Thread th)
        {
            if (context->CallingType == CallingType.Sync)
            {
                var cThread = Scheduler.GetCurrentThread();

                // Connect Threads
                cThread.ChildThread = th;
                cThread.Status = ThreadStatus.Waiting;
                th.ParentThread = cThread;
            }

            //th.StackState->Stack.ECX = arg0;

            th.Start();
            Scheduler.SwitchToThread(th.ThreadID);
        }

        private static string DispatchMessageSymbol = "Lonos.Runtime.MessageManager::Dispatch(Lonos.Kernel.SystemMessage)";

        private static unsafe Addr GetEntryPointFromElf(ElfHelper elf)
        {
            var sym = elf.GetSymbol(DispatchMessageSymbol);
            if (sym == (ElfSymbol*)0)
                return Addr.Zero;
            return sym->Value;
        }

    }

}
