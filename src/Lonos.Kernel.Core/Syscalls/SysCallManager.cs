// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Lonos.Kernel.Core.Collections;
using Lonos.Kernel.Core.Diagnostics;
using Lonos.Kernel.Core.Interrupts;
using Lonos.Kernel.Core.MemoryManagement;
using Lonos.Kernel.Core.PageManagement;
using Lonos.Kernel.Core.Processes;
using Lonos.Kernel.Core.Scheduling;

namespace Lonos.Kernel.Core.SysCalls
{
    public static unsafe class SysCallManager
    {

        public const uint IRQ = 250;

        public static void Setup()
        {
            KernelMessage.WriteLine("Initialize SysCall Manager");

            IDTManager.SetInterruptHandler(IRQ, InterruptHandler);
            IDTManager.SetPrivilegeLevel(IRQ, 0x03);
            IDTManager.Flush();

            Commands = new SysCallInfo[256];
            SetCommands();

            // hack!!
            nextVirtPage = 300 * 1024 * 1024;
        }

        private static void SetCommands()
        {
            SetCommand(SysCallTarget.RequestMemory, Cmd_RequestMemory);
            SetCommand(SysCallTarget.RequestMessageBuffer, Cmd_RequestMessageBuffer);
            SetCommand(SysCallTarget.WriteDebugMessage, Cmd_WriteDebugMessage);
            SetCommand(SysCallTarget.WriteDebugChar, Cmd_WriteDebugChar);
            SetCommand(SysCallTarget.SetThreadPriority, Cmd_SetThreadPriority);
            SetCommand(SysCallTarget.ThreadSleep, Cmd_ThreadSleep);
            SetCommand(SysCallTarget.RegisterService, Cmd_RegisterService);
            SetCommand(SysCallTarget.SetServiceStatus, Cmd_SetServiceStatus);
            SetCommand(SysCallTarget.RegisterInterrupt, Cmd_RegisterInterrupt);
            //SetCommand(SysCallTarget.ServiceFunc1, Cmd_CallServiceFunc1);
            SetCommand(SysCallTarget.GetProcessIDForCommand, Cmd_GetProcessIDForCommand);
            SetCommand(SysCallTarget.ServiceReturn, Cmd_ServiceReturn);
            SetCommand(SysCallTarget.GetPhysicalMemory, Cmd_GetPhysicalMemory);
            SetCommand(SysCallTarget.TranslateVirtualToPhysicalAddress, Cmd_TranslateVirtualToPhysicalAddress);
            SetCommand(SysCallTarget.CreateMemoryProcess, Cmd_CreateMemoryProcess);
        }

        private static uint nextVirtPage;

        private static uint Cmd_RegisteredService(SystemMessage* args)
        {
            var commandNum = GetCommandNum(args->Target);
            var targetProcess = Commands[commandNum].Process;
            if (targetProcess.Service != null)
                targetProcess.Service.SwitchToThreadMethod(args);

            // Normally, code should never reached
            return 0;
        }

        private static uint Cmd_RequestMemory(SystemMessage* args)
        {
            var pages = KMath.DivCeil(args->Arg1, 4096);
            var page = PhysicalPageManager.AllocatePages(pages);
            var virtAddr = nextVirtPage;
            nextVirtPage += pages * 4096;
            Scheduler.GetCurrentThread().Process.PageTable.Map(virtAddr, PhysicalPageManager.GetAddress(page), pages * 4096);
            return virtAddr;
        }

        private static uint Cmd_GetPhysicalMemory(SystemMessage* args)
        {
            var physAddr = args->Arg1;
            var pages = KMath.DivCeil(args->Arg2, 4096);
            var virtAddr = nextVirtPage;
            nextVirtPage += pages * 4096;
            Scheduler.GetCurrentThread().Process.PageTable.Map(virtAddr, physAddr, pages * 4096);
            return virtAddr;
        }

        private static uint Cmd_TranslateVirtualToPhysicalAddress(SystemMessage* args)
        {
            var virtAddr = args->Arg1;
            return Scheduler.GetCurrentThread().Process.PageTable.GetPhysicalAddressFromVirtual(virtAddr);
        }

        private static uint Cmd_RequestMessageBuffer(SystemMessage* args)
        {
            var size = args->Arg1;
            var targetProcessID = args->Arg2;
            var pages = KMath.DivCeil(size, 4096);
            var page = PhysicalPageManager.AllocatePages(pages);
            var virtAddr = nextVirtPage;
            nextVirtPage += pages * 4096;
            Scheduler.GetCurrentThread().Process.PageTable.Map(virtAddr, PhysicalPageManager.GetAddress(page), pages * 4096);
            var targetProc = ProcessManager.System;
            if (targetProcessID > 0)
                targetProc = ProcessManager.GetProcess(targetProcessID);
            targetProc.PageTable.Map(virtAddr, PhysicalPageManager.GetAddress(page), pages * 4096);

            // TODO: implement TargetProcess.RegisterMessageBuffer, because of individual VirtAddr

            return virtAddr;
        }

        private static uint Cmd_WriteDebugMessage(SystemMessage* args)
        {
            var msg = (NullTerminatedString*)args->Arg1;
            KernelMessage.WriteLine(msg);

            return 0;
        }

        private static uint Cmd_WriteDebugChar(SystemMessage* args)
        {
            var c = (char)args->Arg1;
            KernelMessage.Write(c);
            return 0;
        }

        private static uint Cmd_CreateMemoryProcess(SystemMessage* args)
        {
            ProcessManager.StartProcessFromBuffer(args->Arg1);
            //ProcessManager.StartProcess("App.Shell");

            return 0;
        }

        private static uint Cmd_SetThreadPriority(SystemMessage* args)
        {
            Scheduler.SetThreadPriority((int)args->Arg1);

            return 0;
        }

        private static uint Cmd_ThreadSleep(SystemMessage* args)
        {
            var time = args->Arg1;
            Scheduler.Sleep(time);

            return 0;
        }

        private static uint Cmd_RegisterService(SystemMessage* args)
        {
            var proc = Scheduler.GetCurrentThread().Process;
            if (proc.Service != null)
                SetCommand((SysCallTarget)args->Arg1, Cmd_RegisteredService, proc);

            return 0;
        }

        private static uint Cmd_RegisterInterrupt(SystemMessage* args)
        {
            var proc = Scheduler.GetCurrentThread().Process;
            if (proc.Service != null)
            {
                IDTManager.SetInterruptHandler(args->Arg1, InterruptHandlers.Service, proc.Service);
            }

            return 0;
        }

        private static uint Cmd_SetServiceStatus(SystemMessage* args)
        {
            var proc = Scheduler.GetCurrentThread().Process;
            if (proc.Service != null)
                proc.Service.Status = (ServiceStatus)args->Arg1;

            return 0;
        }

        private static uint Cmd_GetProcessIDForCommand(SystemMessage* args)
        {
            var cmdNum = GetCommandNum((SysCallTarget)args->Arg1);
            var proc = Commands[cmdNum].Process;
            if (proc == null)
                proc = ProcessManager.System;
            KernelMessage.WriteLine("Return ProcessID {0} for Command {1}", proc.ProcessID, cmdNum);
            return proc.ProcessID;
        }

        private static uint Cmd_ServiceReturn(SystemMessage* args)
        {
            var servThread = Scheduler.GetCurrentThread();
            var parent = servThread.ParentThread;

            servThread.ParentThread = null;
            parent.ChildThread = null;

            servThread.Status = ThreadStatus.Terminated;

            if (parent.StackState != null)
                parent.StackState->Stack.EAX = args->Arg1;

            Scheduler.SwitchToThread(parent.ThreadID);

            return 0;
        }

        public static void SetCommand(SysCallTarget command, DSysCallInfoHandler handler, Process proc = null)
        {
            Commands[(uint)command] = new SysCallInfo
            {
                CommandID = command,
                Handler = handler,
                Process = proc,
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

        public static void InterruptHandler(IDTStack* stack)
        {
            var args = new SystemMessage
            {
                Target = (SysCallTarget)stack->EAX,
                Arg1 = stack->EBX,
                Arg2 = stack->ECX,
                Arg3 = stack->EDX,
                Arg4 = stack->ESI,
                Arg5 = stack->EDI,
                Arg6 = stack->EBP,
            };

            var commandNum = GetCommandNum(args.Target);

            if (KConfig.Log.SysCall)
                KernelMessage.WriteLine("Got SysCall cmd={0} arg1={1} arg2={2} arg3={3} arg4={4} arg5={5} arg6={6}", (uint)args.Target, args.Arg1, args.Arg2, args.Arg3, args.Arg4, args.Arg5, args.Arg6);

            Scheduler.SaveThreadState(Scheduler.GetCurrentThread().ThreadID, (IntPtr)stack);

            var info = Commands[commandNum];
            if (info == null)
                Panic.Error("Undefined SysCall");

            stack->EAX = info.Handler(&args);
        }

        private static SysCallInfo[] Commands;
    }

    public unsafe delegate uint DSysCallInfoHandler(SystemMessage* args);

    public class SysCallInfo
    {
        public SysCallTarget CommandID;
        //public uint Arguments;
        //public string Name;
        public DSysCallInfoHandler Handler;
        public Process Process;
    }

    //[StructLayout(LayoutKind.Sequential, Size = 4 * 5)]
    //public struct ServiceArgs
    //{
    //    public const uint Size = 4 * 5;

    //    public uint Arg1;
    //    public uint Arg2;
    //    public uint Arg3;
    //    public uint Arg4;
    //    public uint Arg5;
    //}

}
