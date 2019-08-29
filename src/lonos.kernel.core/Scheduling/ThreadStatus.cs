using System;

namespace lonos.kernel.core
{
    public enum ThreadStatus
    {
        Empty = 0,
        Creating,
        Created,
        Running,
        Terminated,
        Waiting
    };

    internal unsafe class Thread : IDisposable
    {
        public ThreadStatus Status = ThreadStatus.Empty;
        public IntPtr StackBottom;
        public IntPtr StackTop;
        //public IntPtr StackStatePointer;
        public IDTTaskStack* StackState;
        public uint DataSelector;
        public uint Ticks;
        public bool User;

        public void Dispose()
        {
            RawVirtualFrameAllocator.FreeRawVirtalMemoryPages(StackTop);
            if (User)
                Memory.Free(StackState);
        }
    }
}
