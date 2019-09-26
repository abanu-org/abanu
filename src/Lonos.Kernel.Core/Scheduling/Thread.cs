// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using Lonos.Kernel.Core.Interrupts;
using Lonos.Kernel.Core.MemoryManagement;
using Lonos.Kernel.Core.PageManagement;
using Lonos.Kernel.Core.Processes;
using Mosa.Runtime;
using Mosa.Runtime.x86;

namespace Lonos.Kernel.Core.Scheduling
{
    public unsafe class Thread
    {
        public ThreadStatus Status = ThreadStatus.Empty;
        public Addr StackBottom;
        public Addr StackTop;
        //public IntPtr StackStatePointer;
        public IDTTaskStack* StackState;
        public Addr KernelStack = null;
        public Addr KernelStackBottom = null;
        public USize KernelStackSize = null;
        public uint Ticks;
        public bool User;
        public Process Process;
        public uint DataSelector;
        public uint ThreadID;
        public bool Debug;
        public string DebugName;
        public uint ArgumentBufferSize;

        /// <summary>
        /// 0: Default
        /// >0: Get Priority more interrupts
        /// <0: Skipping Priority interrupts
        /// </summary>
        public int Priority;
        internal int PriorityInterrupts;

        public Thread ChildThread;
        public Thread ParentThread;

        public void SetArgument(uint offsetBytes, uint value)
        {
            var argAddr = (uint*)GetArgumentAddr(offsetBytes);
            argAddr[0] = value;
        }

        public Addr GetArgumentAddr(uint offsetBytes)
        {
            return StackBottom - ArgumentBufferSize + offsetBytes - 4;
        }

        public void FreeMemory()
        {
            RawVirtualFrameAllocator.FreeRawVirtalMemoryPages(StackTop);
            if (User)
                RawVirtualFrameAllocator.FreeRawVirtalMemoryPages(StackState);
            RawVirtualFrameAllocator.FreeRawVirtalMemoryPages(KernelStack);
        }

        public void Start()
        {
            Status = ThreadStatus.ScheduleForStart;
        }
    }

    public class KernelThread : Thread
    {
    }

    public class UserThread : Thread
    {
    }

}
