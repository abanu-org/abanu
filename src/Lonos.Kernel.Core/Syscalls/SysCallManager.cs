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
using static Lonos.Kernel.Core.Processes.Process;

namespace Lonos.Kernel.Core.SysCalls
{

    public enum CallingType : byte
    {
        Sync,
        Async,
    }

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

            Commands = new SysCallInfo[256];
            SetCommands();
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
            SetCommand(SysCallTarget.GetProcessByName, Cmd_GetProcessByName);
            SetCommand(SysCallTarget.KillProcess, Cmd_KillProcess);
            SetCommand(SysCallTarget.ServiceReturn, Cmd_ServiceReturn);
            SetCommand(SysCallTarget.GetPhysicalMemory, Cmd_GetPhysicalMemory);
            SetCommand(SysCallTarget.TranslateVirtualToPhysicalAddress, Cmd_TranslateVirtualToPhysicalAddress);
            SetCommand(SysCallTarget.CreateMemoryProcess, Cmd_CreateMemoryProcess);
        }

        private static void FunctionInterruptHandler(IDTStack* stack)
        {
            InterruptHandler(stack, CallingType.Sync);
        }

        private static void ActionInterruptHandler(IDTStack* stack)
        {
            InterruptHandler(stack, CallingType.Async);
        }

        private static void InterruptHandler(IDTStack* stack, CallingType callingMethod)
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

            var ctx = new SysCallContext
            {
                CallingType = callingMethod,
            };

            stack->EAX = info.Handler(&ctx, &args);
        }

        private static SysCallInfo[] Commands;

        private static uint Cmd_RegisteredService(SysCallContext* context, SystemMessage* args)
        {
            var commandNum = GetCommandNum(args->Target);
            var targetProcess = Commands[commandNum].Process;
            if (targetProcess.Service != null)
                targetProcess.Service.SwitchToThreadMethod(context, args);

            // Will reach only, if callingMethod==Action
            return 0;
        }

        private static uint Cmd_RequestMemory(SysCallContext* context, SystemMessage* args)
        {
            var size = args->Arg1;
            size = KMath.AlignValueCeil(size, 4096);
            var proc = Scheduler.GetCurrentThread().Process;
            var map = PhysicalPageManager.AllocateRegion(size);
            var virtAddr = proc.UserPageAllocator.AllocatePagesAddr(size / 4096);
            Scheduler.GetCurrentThread().Process.PageTable.Map(virtAddr, map.Start, PhysicalPageManager.GetAllocatorByAddr(map.Start));
            return virtAddr;
        }

        private static uint Cmd_GetPhysicalMemory(SysCallContext* context, SystemMessage* args)
        {
            var physAddr = args->Arg1;
            var pages = KMath.DivCeil(args->Arg2, 4096);
            var proc = Scheduler.GetCurrentThread().Process;
            var virtAddr = proc.UserPageAllocator.AllocatePagesAddr(pages);
            proc.PageTable.Map(virtAddr, physAddr, pages * 4096);
            return virtAddr;
        }

        private static uint Cmd_TranslateVirtualToPhysicalAddress(SysCallContext* context, SystemMessage* args)
        {
            var virtAddr = args->Arg1;
            return Scheduler.GetCurrentThread().Process.PageTable.GetPhysicalAddressFromVirtual(virtAddr);
        }

        private static uint Cmd_RequestMessageBuffer(SysCallContext* context, SystemMessage* args)
        {
            var size = args->Arg1;
            var targetProcessID = (int)args->Arg2;
            var pages = KMath.DivCeil(size, 4096);

            var currentProc = Scheduler.GetCurrentThread().Process;
            var tableCurrent = currentProc.PageTable;

            var targetProc = ProcessManager.System;
            if (targetProcessID > 0)
                targetProc = ProcessManager.GetProcess(targetProcessID);
            var tableTarget = targetProc.PageTable;

            var virtHead = VirtualPageManager.AllocatePages(
                pages,
                new AllocatePageOptions
                {
                    Pool = PageAllocationPool.Global,
                });

            var virtAddr = virtHead;

            for (var pageIdx = 0; pageIdx < pages; pageIdx++)
            {
                var physAddr = PageTable.KernelTable.GetPhysicalAddressFromVirtual(virtAddr);

                if (tableCurrent != PageTable.KernelTable)
                    tableCurrent.Map(virtAddr, physAddr, flush: true);

                if (tableTarget != PageTable.KernelTable)
                    tableTarget.Map(virtAddr, physAddr, flush: true);

                virtAddr += 4096;
            }

            // TODO: implement TargetProcess.RegisterMessageBuffer, because of individual VirtAddr

            currentProc.GlobalAllocations.Add(new GlobalAllocation { Addr = virtHead, TargetProcID = targetProcessID });

            return virtHead;
        }

        private static uint Cmd_WriteDebugMessage(SysCallContext* context, SystemMessage* args)
        {
            var msg = (NullTerminatedString*)args->Arg1;
            KernelMessage.WriteLine(msg);

            return 0;
        }

        private static uint Cmd_WriteDebugChar(SysCallContext* context, SystemMessage* args)
        {
            var c = (char)args->Arg1;
            KernelMessage.Write(c);
            return 0;
        }

        private static uint Cmd_CreateMemoryProcess(SysCallContext* context, SystemMessage* args)
        {
            var addr = args->Arg1;
            var size = args->Arg2;
            ProcessManager.StartProcessFromBuffer(new MemoryRegion(addr, size));
            //ProcessManager.StartProcess("App.Shell");

            return 0;
        }

        private static uint Cmd_GetProcessByName(SysCallContext* context, SystemMessage* args)
        {
            var name = (NullTerminatedString*)args->Arg1;

            var proc = ProcessManager.GetProcessByName(name);
            if (proc != null)
                return (uint)proc.ProcessID;

            return unchecked((uint)-1);
        }

        private static uint Cmd_KillProcess(SysCallContext* context, SystemMessage* args)
        {
            var procId = (int)args->Arg1;
            var currentProcId = Scheduler.GetCurrentThread().Process.ProcessID;
            ProcessManager.KillProcess(procId);

            if (procId == currentProcId)
                Scheduler.ScheduleNextThread();
            return 0;
        }

        private static uint Cmd_SetThreadPriority(SysCallContext* context, SystemMessage* args)
        {
            Scheduler.SetThreadPriority((int)args->Arg1);

            return 0;
        }

        private static uint Cmd_ThreadSleep(SysCallContext* context, SystemMessage* args)
        {
            var time = args->Arg1;
            Scheduler.Sleep(time);

            return 0;
        }

        private static uint Cmd_RegisterService(SysCallContext* context, SystemMessage* args)
        {
            var proc = Scheduler.GetCurrentThread().Process;
            if (proc.Service != null)
                SetCommand((SysCallTarget)args->Arg1, Cmd_RegisteredService, proc);

            return 0;
        }

        private static uint Cmd_RegisterInterrupt(SysCallContext* context, SystemMessage* args)
        {
            var proc = Scheduler.GetCurrentThread().Process;
            if (proc.Service != null)
            {
                IDTManager.SetInterruptHandler(args->Arg1, InterruptHandlers.Service, proc.Service);
            }

            return 0;
        }

        private static uint Cmd_SetServiceStatus(SysCallContext* context, SystemMessage* args)
        {
            var proc = Scheduler.GetCurrentThread().Process;
            if (proc.Service != null)
                proc.Service.Status = (ServiceStatus)args->Arg1;

            return 0;
        }

        private static uint Cmd_GetProcessIDForCommand(SysCallContext* context, SystemMessage* args)
        {
            var cmdNum = GetCommandNum((SysCallTarget)args->Arg1);
            var proc = Commands[cmdNum].Process;
            if (proc == null)
                proc = ProcessManager.System;
            KernelMessage.WriteLine("Return ProcessID {0} for Command {1}", proc.ProcessID, cmdNum);
            return (uint)proc.ProcessID;
        }

        private static uint Cmd_ServiceReturn(SysCallContext* context, SystemMessage* args)
        {
            var servThread = Scheduler.GetCurrentThread();
            servThread.Status = ThreadStatus.Terminated;

            if (servThread.ParentThread != null)
            {
                var parent = servThread.ParentThread;

                servThread.ParentThread = null;
                parent.ChildThread = null;

                if (parent.StackState != null)
                    parent.StackState->Stack.EAX = args->Arg1;

                Scheduler.SwitchToThread(parent.ThreadID);
            }
            Scheduler.ScheduleNextThread();
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

    }

    public struct SysCallContext
    {
        public CallingType CallingType;
    }

    public unsafe delegate uint DSysCallInfoHandler(SysCallContext* context, SystemMessage* args);

    public class SysCallInfo
    {
        public SysCallTarget CommandID;
        //public uint Arguments;
        //public string Name;
        public DSysCallInfoHandler Handler;
        public Process Process;
    }

}
