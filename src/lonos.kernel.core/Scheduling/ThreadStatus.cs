using System;

namespace lonos.kernel.core
{
    public enum ThreadStatus
    {
        Empty = 0,
        Creating,
        Running,
        Terminating,
        Terminated,
        Waiting
    };

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
    }
}
