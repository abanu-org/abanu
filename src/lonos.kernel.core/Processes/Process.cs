﻿using lonos.Kernel.Core.Collections;
using lonos.Kernel.Core.PageManagement;
using lonos.Kernel.Core.Scheduling;

namespace lonos.Kernel.Core.Processes
{
    public class Process
    {

        public uint ProcessID;
        public KList<Thread> Threads;
        public bool User;
        public string Path;
        public IPageTable PageTable;

        public Process()
        {
            Threads = new KList<Thread>(1);
        }

        public void Start()
        {
            lock (Threads)
                for (var i = 0; i < Threads.Count; i++)
                    Threads[i].Start();
        }

    }

}