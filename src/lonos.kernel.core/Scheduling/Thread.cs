using lonos.Kernel.Core.Interrupts;
using lonos.Kernel.Core.MemoryManagement;
using System;

namespace lonos.Kernel.Core.Scheduling
{
    internal unsafe class Thread
    {
        public ThreadStatus Status = ThreadStatus.Empty;
        public IntPtr StackBottom;
        public IntPtr StackTop;
        //public IntPtr StackStatePointer;
        public IDTTaskStack* StackState;
        public uint DataSelector;
        public uint Ticks;
        public bool User;

        public void FreeMemory()
        {
            RawVirtualFrameAllocator.FreeRawVirtalMemoryPages(StackTop);
            if (User)
                Memory.Free(StackState);
        }
    }
}
