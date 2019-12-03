// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System;
using Abanu.Kernel.Core.Collections;
using Abanu.Kernel.Core.MemoryManagement;
using Abanu.Kernel.Core.PageManagement;
using Abanu.Kernel.Core.Scheduling;

namespace Abanu.Kernel.Core.Processes
{

    /// <summary>
    /// Represents a Process
    /// </summary>
    public class Process : IDisposable
    {

        /// <summary>
        /// Unique Process Identifier
        /// </summary>
        public int ProcessID;

        public ProcessRunState RunState;

        public KList<Thread> Threads;
        public KList<GlobalAllocation> GlobalAllocations;

        /// <summary>
        /// Determines if this the process has user or kernel privileges.
        /// This may not be the same as <see cref="IsKernelProcess"/>.
        /// </summary>
        public bool User;

        /// <summary>
        /// Path to executable
        /// </summary>
        public string Path;

        public IPageTable PageTable;
        internal Addr PageTableAllocAddr;

        public ProcessService Service;

        public IPageFrameAllocator UserPageAllocator;

        public Addr UserElfSectionsAddr;

        /// <summary>
        /// Determines if this process is a user process or a kernel process.
        /// This may not be the same as <see cref="User"/>.
        /// </summary>
        public bool IsKernelProcess => PageTable == PageManagement.PageTable.KernelTable;

        internal uint CurrentBrk = 0;
        internal uint BrkBase;

        public Process()
        {
            Threads = new KList<Thread>(1);
            GlobalAllocations = new KList<GlobalAllocation>();
            //StdIn = new FifoQueue<byte>(256);
            CurrentBrk = 0;
        }

        public void Dispose()
        {
            //Memory.FreeObject(StdIn);
            VirtualPageManager.FreeAddr(PageTableAllocAddr);
            Threads.Dispose();
            GlobalAllocations.Dispose();
        }

        public void Start()
        {
            UninterruptibleMonitor.Enter(Threads);
            try
            {
                RunState = ProcessRunState.Running;
                for (var i = 0; i < Threads.Count; i++)
                {
                    KernelMessage.WriteLine("Starting {0}, ProcessID={1} on Thread {2}", Path, (uint)ProcessID, (uint)Threads[0].ThreadID);
                    Threads[i].Start();
                }
            }
            finally
            {
                UninterruptibleMonitor.Exit(Threads);
            }

        }

        public struct GlobalAllocation
        {
            public Addr Addr;
            public int TargetProcID;
        }

    }

    public enum ProcessRunState
    {
        Creating,
        Running,
        Terminated,
    }

}
