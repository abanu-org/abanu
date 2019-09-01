using lonos.Kernel.Core.Interrupts;
using lonos.Kernel.Core.MemoryManagement;
using lonos.Kernel.Core.PageManagement;
using lonos.Kernel.Core.Processes;
using System;

namespace lonos.Kernel.Core.Scheduling
{
    public unsafe class Thread
    {
        public ThreadStatus Status = ThreadStatus.Empty;
        public IntPtr StackBottom;
        public IntPtr StackTop;
        //public IntPtr StackStatePointer;
        public IDTTaskStack* StackState;
        public uint Ticks;
        public bool User;
        public Process Process;
        public uint DataSelector;
        public uint ThreadID;
        public bool Debug;
        public string DebugName;

        public void FreeMemory()
        {
            RawVirtualFrameAllocator.FreeRawVirtalMemoryPages(StackTop);
            if (User)
                RawVirtualFrameAllocator.FreeRawVirtalMemoryPages(StackState);
        }

        public void Start()
        {
            Status = ThreadStatus.ScheduleForStart;
        }
    }
}
