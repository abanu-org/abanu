// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using Lonos.Kernel.Core.Collections;
using Lonos.Kernel.Core.MemoryManagement;
using Lonos.Kernel.Core.PageManagement;
using Lonos.Kernel.Core.Scheduling;

namespace Lonos.Kernel.Core.Processes
{

    public class Process : IDisposable
    {

        public uint ProcessID;
        public KList<Thread> Threads;
        public bool User;
        public string Path;
        public IPageTable PageTable;
        public Service Service;
        //public FifoQueue<byte> StdIn;
        internal Addr PageTableAllocAddr;

        public Process()
        {
            Threads = new KList<Thread>(1);
            //StdIn = new FifoQueue<byte>(256);
        }

        public void Dispose()
        {
            //Memory.FreeObject(StdIn);
            VirtualPageManager.FreeAddrIdentity(PageTableAllocAddr);
        }

        public void Start()
        {
            lock (Threads)
                for (var i = 0; i < Threads.Count; i++)
                    Threads[i].Start();
        }

    }

}
