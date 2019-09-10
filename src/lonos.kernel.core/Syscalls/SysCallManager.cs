// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using lonos.Kernel.Core.Collections;
using lonos.Kernel.Core.Interrupts;
using lonos.Kernel.Core.MemoryManagement;
using lonos.Kernel.Core.PageManagement;
using lonos.Kernel.Core.Processes;
using lonos.Kernel.Core.Scheduling;

namespace lonos.Kernel.Core.SysCalls
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
            SetCommand(SysCallTarget.ServiceFunc1, Cmd_CallServiceFunc1);
            SetCommand(SysCallTarget.GetProcessIDForCommand, Cmd_GetProcessIDForCommand);
            SetCommand(SysCallTarget.ServiceReturn, Cmd_ServiceReturn);
            SetCommand(SysCallTarget.GetPhysicalMemory, Cmd_GetPhysicalMemory);
            SetCommand(SysCallTarget.TranslateVirtualToPhysicalAddress, Cmd_TranslateVirtualToPhysicalAddress);
        }

        private static uint nextVirtPage;

        private static uint Cmd_RequestMemory(SystemMessage* args)
        {
            var pages = KMath.DivCeil(args->Arg1, 4096);
            var page = PageFrameManager.AllocatePages(PageFrameRequestFlags.Default, pages);
            var virtAddr = nextVirtPage;
            nextVirtPage += (pages * 4096);
            Scheduler.GetCurrentThread().Process.PageTable.Map(virtAddr, page->PhysicalAddress, pages * 4096);
            return virtAddr;
        }

        private static uint Cmd_GetPhysicalMemory(SystemMessage* args)
        {
            var physAddr = args->Arg1;
            var pages = KMath.DivCeil(args->Arg2, 4096);
            var virtAddr = nextVirtPage;
            nextVirtPage += (pages * 4096);
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
            var page = PageFrameManager.AllocatePages(PageFrameRequestFlags.Default, pages);
            var virtAddr = nextVirtPage;
            nextVirtPage += (pages * 4096);
            Scheduler.GetCurrentThread().Process.PageTable.Map(virtAddr, page->PhysicalAddress, pages * 4096);
            var targetProc = ProcessManager.System;
            if (targetProcessID > 0)
                targetProc = ProcessManager.GetProcess(targetProcessID);
            targetProc.PageTable.Map(virtAddr, page->PhysicalAddress, pages * 4096);

            // TODO: implement TargetProcess.RegisterMessageBuffer, because of individual VirtAddr

            return virtAddr;
        }

        private static uint Cmd_WriteDebugMessage(SystemMessage* args)
        {
            // TODO: Security
            var start = args->Arg1;
            var length = args->Arg2;
            var data = (char*)start;

            for (var i = 0; i < length; i++)
                KernelMessage.Write(data[i]);

            return 0;
        }

        private static uint Cmd_WriteDebugChar(SystemMessage* args)
        {
            var c = (char)args->Arg1;
            KernelMessage.Write(c);
            return 0;
        }

        private static uint Cmd_CallServiceFunc1(SystemMessage* args)
        {
            var serv = KernelStart.serv;

            //0xAABBCCDD

            //var servArgs = new ServiceArgs
            //{
            //    Arg1 = args.Arg2,
            //    Arg2 = args.Arg3,
            //    Arg3 = args.Arg4,
            //    Arg4 = args.Arg5,
            //    Arg5 = args.Arg6,
            //};

            serv.SwitchToThreadMethod(args);

            // will never get here, because service will call cmd_ExitServiceFunc, thats switching this this thread directly
            return 0;
        }

        private static uint Cmd_GetProcessIDForCommand(SystemMessage* args)
        {
            var proc = Commands[GetCommandNum(args->Target)].Process;
            if (proc == null)
                proc = ProcessManager.System;
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
                parent.StackState->Stack.EAX = args->Arg2;

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

            if (KConfig.TraceSysCall)
                KernelMessage.WriteLine("Got SysCall cmd={0} arg1={1} arg2={2} arg3={3} arg4={4} arg5={5} arg6={6}", (uint)args.Target, args.Arg1, args.Arg2, args.Arg3, args.Arg4, args.Arg5, args.Arg6);

            Scheduler.SaveThreadState(Scheduler.GetCurrentThread().ThreadID, (IntPtr)stack);

            stack->EAX = Commands[commandNum].Handler(&args);
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
