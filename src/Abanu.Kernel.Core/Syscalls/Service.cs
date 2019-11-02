// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using Abanu.Kernel.Core.Elf;
using Abanu.Kernel.Core.Processes;
using Abanu.Kernel.Core.SysCalls;

namespace Abanu.Kernel.Core.Scheduling
{
    public class Service
    {

        public Process Process;
        public ServiceStatus Status;

        private Addr DefaultDispatchEntryPoint;

        public Service(Process proc)
        {
            Process = proc;
            Status = ServiceStatus.NotInizialized;
        }

        internal void Init(Addr defaultDispatchEntryPoint)
        {
            DefaultDispatchEntryPoint = defaultDispatchEntryPoint;
        }

        // Methods is always called within Interrupt with Interrupt disabled

        // TODO: Code duplication! Both SwitchToThreadMethod are very similar.

        public unsafe void SwitchToThreadMethod(SysCallContext* context, SystemMessage* args)
        {
            var th = CreateThread(DefaultDispatchEntryPoint, SystemMessage.Size);
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

    }

}
