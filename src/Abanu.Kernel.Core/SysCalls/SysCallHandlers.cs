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
    /// <summary>
    /// Implementation of Kernel side handlers.
    /// User side handlers will be handled via <see cref="RegisteredService(ref SysCallContext, ref SystemMessage)"/>
    /// </summary>
    internal static class SysCallHandlers
    {

        internal static unsafe uint ServiceReturn(ref SysCallContext context, ref SystemMessage args)
        {
            var servThread = Scheduler.GetCurrentThread();
            servThread.Status = ThreadStatus.Terminated;

            if (servThread.ParentThread != null)
            {
                var parent = servThread.ParentThread;

                servThread.ParentThread = null;
                parent.ChildThread = null;

                if (parent.StackState != null)
                    parent.StackState->Stack.EAX = args.Arg1;

                Scheduler.SwitchToThread(parent.ThreadID);
            }
            Scheduler.ScheduleNextThread();
            return 0;
        }

        /// <summary>
        /// Command is handled by another Service (up call)
        /// </summary>
        internal static uint RegisteredService(ref SysCallContext context, ref SystemMessage args)
        {
            var handler = SysCallManager.GetHandler(args.Target);
            var targetProcess = handler.Process;
            if (targetProcess.Service != null)
                targetProcess.Service.SwitchToThreadMethod(ref context, ref args);

            // Will reach only, if callingMethod==Action
            return 0;
        }

        internal static uint RequestMemory(ref SysCallContext context, ref SystemMessage args)
        {
            var size = args.Arg1;
            size = KMath.AlignValueCeil(size, 4096);
            var proc = Scheduler.GetCurrentThread().Process;
            var map = PhysicalPageManager.AllocateRegion(size);
            var virtAddr = proc.UserPageAllocator.AllocatePagesAddr(size / 4096);
            Scheduler.GetCurrentThread().Process.PageTable.Map(virtAddr, map.Start, PhysicalPageManager.GetAllocatorByAddr(map.Start));
            return virtAddr;
        }

        internal static uint GetPhysicalMemory(ref SysCallContext context, ref SystemMessage args)
        {
            var physAddr = args.Arg1;
            var pages = KMath.DivCeil(args.Arg2, 4096);
            KernelMessage.WriteLine("Got Request for {0:X8} pages at Physical Addr {1:X8}", pages, physAddr);
            var proc = Scheduler.GetCurrentThread().Process;
            var virtAddr = proc.UserPageAllocator.AllocatePagesAddr(pages);
            proc.PageTable.Map(virtAddr, physAddr, pages * 4096);
            return virtAddr;
        }

        internal static uint TranslateVirtualToPhysicalAddress(ref SysCallContext context, ref SystemMessage args)
        {
            var virtAddr = args.Arg1;
            return Scheduler.GetCurrentThread().Process.PageTable.GetPhysicalAddressFromVirtual(virtAddr);
        }

        internal static uint GetElfSectionsAddress(ref SysCallContext context, ref SystemMessage args)
        {
            var proc = Scheduler.GetCurrentThread().Process;
            return proc.UserElfSectionsAddr;
        }

        internal static unsafe uint GetFramebufferInfo(ref SysCallContext context, ref SystemMessage args)
        {
            var virtAddr = args.Arg1;
            var virtPresent = (int*)virtAddr;
            *virtPresent = Boot.BootInfo.Header->FBPresent ? 1 : 0;
            var virtInfo = (BootInfoFramebufferInfo*)(virtAddr + 4);
            *virtInfo = Boot.BootInfo.Header->FbInfo;
            return 0;
        }

        internal static uint RequestMessageBuffer(ref SysCallContext context, ref SystemMessage args)
        {
            var size = args.Arg1;
            var targetProcessID = (int)args.Arg2;
            var pages = KMath.DivCeil(size, 4096);

            var currentProc = Scheduler.GetCurrentThread().Process;
            var tableCurrent = currentProc.PageTable;

            var targetProc = ProcessManager.System;
            if (targetProcessID > 0)
                targetProc = ProcessManager.GetProcessByID(targetProcessID);
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

        internal static unsafe uint WriteDebugMessage(ref SysCallContext context, ref SystemMessage args)
        {
            var msg = (NullTerminatedString*)args.Arg1;
            KernelMessage.WriteLine(msg);

            return 0;
        }

        internal static uint WriteDebugChar(ref SysCallContext context, ref SystemMessage args)
        {
            var c = (char)args.Arg1;
            KernelMessage.Write(c);
            return 0;
        }

        internal static uint CreateMemoryProcess(ref SysCallContext context, ref SystemMessage args)
        {
            var addr = args.Arg1;
            var size = args.Arg2;
            ProcessManager.StartProcessFromBuffer(new MemoryRegion(addr, size));
            //ProcessManager.StartProcess("App.Shell");

            return 0;
        }

        internal static unsafe uint GetProcessByName(ref SysCallContext context, ref SystemMessage args)
        {
            var name = (NullTerminatedString*)args.Arg1;

            var proc = ProcessManager.GetProcessByName(name);
            if (proc != null)
                return (uint)proc.ProcessID;

            return unchecked((uint)-1);
        }

        internal static uint GetCurrentProcessID(ref SysCallContext context, ref SystemMessage args)
        {
            return (uint)Scheduler.GetCurrentThread().Process.ProcessID;
        }

        internal static uint GetCurrentThreadID(ref SysCallContext context, ref SystemMessage args)
        {
            return (uint)Scheduler.GetCurrentThread().ThreadID;
        }

        internal static uint KillProcess(ref SysCallContext context, ref SystemMessage args)
        {
            var procId = (int)args.Arg1;
            var currentProcId = Scheduler.GetCurrentThread().Process.ProcessID;
            ProcessManager.KillProcessByID(procId);

            if (procId == currentProcId)
                Scheduler.ScheduleNextThread();
            return 0;
        }

        internal static uint SetThreadPriority(ref SysCallContext context, ref SystemMessage args)
        {
            Scheduler.SetThreadPriority((int)args.Arg1);

            return 0;
        }

        internal static uint ThreadSleep(ref SysCallContext context, ref SystemMessage args)
        {
            var time = args.Arg1;
            Scheduler.Sleep(time);

            return 0;
        }

        internal static uint SetThreadStorageSegmentBase(ref SysCallContext context, ref SystemMessage args)
        {
            var addr = args.Arg1;
            Scheduler.GetCurrentThread().ThreadLocalStorageBaseAddr = addr;
            GDT.SetThreadStorageSegmentBase(addr);

            return 0;
        }

        internal static uint RegisterService(ref SysCallContext context, ref SystemMessage args)
        {
            var proc = Scheduler.GetCurrentThread().Process;
            if (proc.Service != null)
                SysCallManager.SetCommand((SysCallTarget)args.Arg1, RegisteredService, proc);

            return 0;
        }

        internal static uint RegisterInterrupt(ref SysCallContext context, ref SystemMessage args)
        {
            var proc = Scheduler.GetCurrentThread().Process;
            if (proc.Service != null)
            {
                IDTManager.SetInterruptHandler(args.Arg1, InterruptHandlers.Service, proc.Service);
            }

            return 0;
        }

        internal static uint SetServiceStatus(ref SysCallContext context, ref SystemMessage args)
        {
            var proc = Scheduler.GetCurrentThread().Process;
            if (proc.Service != null)
                proc.Service.Status = (ServiceStatus)args.Arg1;

            return 0;
        }

        internal static uint GetProcessIDForCommand(ref SysCallContext context, ref SystemMessage args)
        {
            var handler = SysCallManager.GetHandler((SysCallTarget)args.Arg1);
            var proc = handler.Process;
            if (proc == null)
                proc = ProcessManager.System;

            KernelMessage.WriteLine("Return ProcessID {0} for Command {1}", proc.ProcessID, (uint)handler.CommandID);
            return (uint)proc.ProcessID;
        }

    }
}
