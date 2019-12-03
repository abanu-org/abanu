// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Abanu.Kernel.Core.Collections;
using Abanu.Kernel.Core.Diagnostics;
using Abanu.Kernel.Core.Interrupts;
using Abanu.Kernel.Core.MemoryManagement;
using Abanu.Kernel.Core.PageManagement;
using Abanu.Kernel.Core.Processes;
using Abanu.Kernel.Core.Scheduling;
using static Abanu.Kernel.Core.Processes.Process;

namespace Abanu.Kernel.Core.SysCalls
{

    public static unsafe class SysCallManager
    {

        public const uint FunctionIRQ = 250;
        public const uint ActionIRQ = 251;

        public static void Setup()
        {
            KernelMessage.WriteLine("Initialize SysCall Manager");

            IDTManager.SetInterruptHandler(FunctionIRQ, FunctionInterruptHandler);
            IDTManager.SetPrivilegeLevel(FunctionIRQ, 0x03);
            IDTManager.SetInterruptHandler(ActionIRQ, ActionInterruptHandler);
            IDTManager.SetPrivilegeLevel(ActionIRQ, 0x03);
            IDTManager.Flush();

            Commands = new SysCallHandlerInfo[256];
            SetCommands();
        }

        /// <summary>
        /// Assignment of kernel side commands
        /// </summary>
        private static void SetCommands()
        {
            SetCommand(SysCallTarget.RequestMemory, SysCallHandlers.RequestMemory);
            SetCommand(SysCallTarget.Sbrk, SysCallHandlers.Sbrk);
            SetCommand(SysCallTarget.RequestMessageBuffer, SysCallHandlers.RequestMessageBuffer);
            SetCommand(SysCallTarget.WriteDebugMessage, SysCallHandlers.WriteDebugMessage);
            SetCommand(SysCallTarget.WriteDebugChar, SysCallHandlers.WriteDebugChar);
            SetCommand(SysCallTarget.SetThreadPriority, SysCallHandlers.SetThreadPriority);
            SetCommand(SysCallTarget.ThreadSleep, SysCallHandlers.ThreadSleep);
            SetCommand(SysCallTarget.SetThreadStorageSegmentBase, SysCallHandlers.SetThreadStorageSegmentBase);
            SetCommand(SysCallTarget.RegisterService, SysCallHandlers.RegisterService);
            SetCommand(SysCallTarget.SetServiceStatus, SysCallHandlers.SetServiceStatus);
            SetCommand(SysCallTarget.RegisterInterrupt, SysCallHandlers.RegisterInterrupt);
            //SetCommand(SysCallTarget.ServiceFunc1, CallServiceFunc1);
            SetCommand(SysCallTarget.GetProcessIDForCommand, SysCallHandlers.GetProcessIDForCommand);
            SetCommand(SysCallTarget.GetProcessByName, SysCallHandlers.GetProcessByName);
            SetCommand(SysCallTarget.GetCurrentProcessID, SysCallHandlers.GetCurrentProcessID);
            SetCommand(SysCallTarget.GetCurrentThreadID, SysCallHandlers.GetCurrentThreadID);
            SetCommand(SysCallTarget.GetRemoteProcessID, SysCallHandlers.GetRemoteProcessID);
            SetCommand(SysCallTarget.GetRemoteThreadID, SysCallHandlers.GetRemoteThreadID);
            SetCommand(SysCallTarget.KillProcess, SysCallHandlers.KillProcess);
            SetCommand(SysCallTarget.StartProcess, SysCallHandlers.StartProcess);
            SetCommand(SysCallTarget.ServiceReturn, SysCallHandlers.ServiceReturn);
            SetCommand(SysCallTarget.GetPhysicalMemory, SysCallHandlers.GetPhysicalMemory);
            SetCommand(SysCallTarget.TranslateVirtualToPhysicalAddress, SysCallHandlers.TranslateVirtualToPhysicalAddress);
            SetCommand(SysCallTarget.GetElfSectionsAddress, SysCallHandlers.GetElfSectionsAddress);
            SetCommand(SysCallTarget.GetFramebufferInfo, SysCallHandlers.GetFramebufferInfo);
            SetCommand(SysCallTarget.CreateMemoryProcess, SysCallHandlers.CreateMemoryProcess);
        }

        /// <summary>
        /// Syscall interrupt handler for synchronous calls.
        /// </summary>
        private static void FunctionInterruptHandler(ref IDTStack stack)
        {
            InterruptHandler(ref stack, SysCallCallingType.Sync);
        }

        /// <summary>
        /// Syscall interrupt handler for asynchronous calls.
        /// </summary>
        private static void ActionInterruptHandler(ref IDTStack stack)
        {
            InterruptHandler(ref stack, SysCallCallingType.Async);
        }

        /// <summary>
        /// Syscall interrupt handler. Dispatcher for every SysCall.
        /// </summary>
        private static void InterruptHandler(ref IDTStack stack, SysCallCallingType callingMethod)
        {
            var args = new SystemMessage
            {
                Target = (SysCallTarget)stack.EAX,
                Arg1 = stack.EBX,
                Arg2 = stack.ECX,
                Arg3 = stack.EDX,
                Arg4 = stack.ESI,
                Arg5 = stack.EDI,
                Arg6 = stack.EBP,
            };

            var commandNum = GetCommandNum(args.Target);

            if (KConfig.Log.SysCall)
                KernelMessage.WriteLine("Got SysCall cmd={0} arg1={1:X8} arg2={2:X8} arg3={3:X8} arg4={4:X8} arg5={5:X8} arg6={6:X8}", (uint)args.Target, args.Arg1, args.Arg2, args.Arg3, args.Arg4, args.Arg5, args.Arg6);

            Scheduler.SaveThreadState(Scheduler.GetCurrentThread().ThreadID, ref stack);

            var info = Commands[commandNum];
            if (info == null)
                Panic.Error("Undefined SysCall");

            var ctx = new SysCallContext
            {
                CallingType = callingMethod,
                Debug = info.Debug,
            };

            if (info.Debug)
            {
                KDebug.DumpStats();
                Debug.Nop();
            }

            var result = info.Handler(ref ctx, ref args);

            if (KConfig.Log.SysCall)
                KernelMessage.WriteLine("Result of Syscall cmd={0}: {1:X8}", (uint)args.Target, result);

            stack.EAX = result;
        }

        private static SysCallHandlerInfo[] Commands;

        public static void SetCommand(SysCallTarget command, DSysCallInfoHandler handler, Process proc = null)
        {
            var debug = false;
            if (command == SysCallTarget.Tmp_DisplayServer_CreateWindow)
            {
                debug = true;
            }

            if (proc != null)
                KernelMessage.WriteLine("Register Command {0} for Process {1}", (uint)command, (uint)proc.ProcessID);

            Commands[(uint)command] = new SysCallHandlerInfo
            {
                CommandID = command,
                Handler = handler,
                Process = proc,
                Debug = debug,
            };
        }

        public static void SetCommandProcess(SysCallTarget command, Process proc)
        {
            Commands[(uint)command].Process = proc;
        }

        private const uint CommandMask = BitsMask.Bits10;

        private static uint GetCommandNum(SysCallTarget target)
        {
            return (uint)target & CommandMask;
        }

        internal static SysCallHandlerInfo GetHandler(SysCallTarget target)
        {
            var commandNum = GetCommandNum(target);
            return Commands[commandNum];
        }

    }

    public delegate uint DSysCallInfoHandler(ref SysCallContext context, ref SystemMessage args);

}
