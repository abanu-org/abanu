// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using Abanu.Kernel.Core.Collections;
using Abanu.Kernel.Core.MemoryManagement;
using Abanu.Kernel.Core.PageManagement;
using Abanu.Kernel.Core.Scheduling;

namespace Abanu.Kernel.Core.Processes
{

    public class Process : IDisposable
    {

        public int ProcessID;
        public ProcessRunState RunState;
        public KList<Thread> Threads;
        public KList<GlobalAllocation> GlobalAllocations;
        public bool User;
        public string Path;
        public IPageTable PageTable;
        public Service Service;
        //public FifoQueue<byte> StdIn;
        internal Addr PageTableAllocAddr;
        public IPageFrameAllocator UserPageAllocator;
        public Addr UserElfSectionsAddr;

        public bool IsKernelProcess => PageTable == PageManagement.PageTable.KernelTable;

        public Process()
        {
            Threads = new KList<Thread>(1);
            GlobalAllocations = new KList<GlobalAllocation>();
            //StdIn = new FifoQueue<byte>(256);
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
            Service.Init();
            UninterruptableMonitor.Enter(Threads);
            try
            {
                RunState = ProcessRunState.Running;
                for (var i = 0; i < Threads.Count; i++)
                    Threads[i].Start();
            }
            finally
            {
                UninterruptableMonitor.Exit(Threads);
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
