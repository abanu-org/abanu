﻿// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using Abanu.Kernel.Core.Elf;
using Abanu.Kernel.Core.Processes;
using Abanu.Kernel.Core.Scheduling;
using Abanu.Kernel.Core.SysCalls;

namespace Abanu.Kernel.Core.Processes
{
    public class ProcessService
    {

        public Process Process;
        public ServiceStatus Status;

        private Addr DefaultDispatchEntryPoint;

        public ProcessService(Process proc)
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

        public unsafe void SwitchToThreadMethod(ref SysCallContext context, ref SystemMessage args)
        {
            var th = CreateThread(DefaultDispatchEntryPoint, SystemMessage.Size);
            var argsPtr = (SystemMessage*)Unsafe.AsPointer(ref args);
            th.DebugSystemMessage = *argsPtr;
            var argAddr = (SystemMessage*)th.GetArgumentAddr(0);
            argAddr[0] = *argsPtr;
            SwitchToThread(context, th);
        }

        public Thread CreateThread(uint methodAddr, uint argumentBufferSize)
        {
            return Scheduler.CreateThread(Process, new ThreadStartOptions(methodAddr) { ArgumentBufferSize = argumentBufferSize, DebugName = "ServiceCall" });
        }

        public static unsafe void SwitchToThread(in SysCallContext context, Thread th)
        {
            if (context.CallingType == SysCallCallingType.Sync)
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
