// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System;
using Abanu.Kernel.Core.Interrupts;
using Abanu.Kernel.Core.MemoryManagement;
using Abanu.Kernel.Core.PageManagement;
using Abanu.Kernel.Core.Processes;
using Mosa.Runtime;
using Mosa.Runtime.x86;

namespace Abanu.Kernel.Core.Scheduling
{
    public unsafe class Thread
    {
        public ThreadStatus Status = ThreadStatus.Empty;

        /// <summary>
        /// Upper Address of Stack
        /// </summary>
        internal Addr StackBottom;

        /// <summary>
        /// Lower Address of Stack
        /// </summary>
        internal Addr StackTop;

        /// <summary>
        /// Stores thread state on task switch
        /// </summary>
        internal IDTTaskStack* StackState;

        // TODO: Documentation of Kernel Stack

        internal Addr KernelStack = null;
        internal Addr KernelStackBottom = null;
        internal USize KernelStackSize = null;

        /// <summary>
        /// Consumed Interrupts
        /// </summary>
        internal uint Ticks;

        /// <summary>
        /// Determines of this Thread is running with user privileges or kernel privileges
        /// </summary>
        internal bool User;

        /// <summary>
        /// Process where this thread belongs to
        /// </summary>
        public Process Process;

        /// <summary>
        /// Data Segment used for this Thread
        /// </summary>
        internal uint DataSelector;

        /// <summary>
        /// Unique Thread Identifier
        /// </summary>
        public uint ThreadID;

        /// <summary>
        /// If true, this thread will be debugged.
        /// </summary>
        public bool Debug;
        internal string DebugName;
        internal SystemMessage DebugSystemMessage;

        /// <summary>
        /// Required bytes for entrypoint arguments. Used to build the initial stack correctly.
        /// </summary>
        internal uint ArgumentBufferSize;

        public bool CanScheduled
        {
            get
            {
                return Status == ThreadStatus.ScheduleForStart || Status == ThreadStatus.Running;
            }
        }

        /// <summary>
        /// 0: Default
        /// >0: Get <see cref="Priority"/> more interrupts
        /// <0: Skipping <see cref="Priority"/> interrupts
        /// </summary>
        public int Priority;
        internal int PriorityInterrupts;

        /// <summary>
        /// The up call thread. This Thread is waiting for the Child Thread.
        /// </summary>
        public Thread ChildThread;

        /// <summary>
        /// The parent thread, thats waiting for this thread.
        /// </summary>
        public Thread ParentThread;

        /// <summary>
        /// Writes arguments for the entrypoint on the initial stack
        /// </summary>
        public void SetArgument(uint offsetBytes, uint value)
        {
            if (Status != ThreadStatus.Creating)
                throw new InvalidOperationException();

            var argAddr = (uint*)GetArgumentAddr(offsetBytes);
            argAddr[0] = value;
        }

        public Addr GetArgumentAddr(uint offsetBytes)
        {
            return StackBottom - ArgumentBufferSize + offsetBytes - 4;
        }

        public void FreeMemory()
        {
            VirtualPageManager.FreeAddr(StackTop);
            if (User)
                VirtualPageManager.FreeAddr(StackState);
            VirtualPageManager.FreeAddr(KernelStack);
        }

        public void Start()
        {
            Status = ThreadStatus.ScheduleForStart;
        }

        public void Terminate()
        {
            Status = ThreadStatus.Terminated;
            if (ChildThread != null)
            {
                ChildThread.Terminate();
                ChildThread = null;
            }
            ParentThread = null;
        }

    }

    public class KernelThread : Thread
    {
    }

    public class UserThread : Thread
    {
    }

}
