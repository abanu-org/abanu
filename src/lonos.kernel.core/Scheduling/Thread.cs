using System;
using lonos.Kernel.Core.Interrupts;
using lonos.Kernel.Core.MemoryManagement;
using lonos.Kernel.Core.PageManagement;
using lonos.Kernel.Core.Processes;
using Mosa.Runtime;
using Mosa.Runtime.x86;

namespace lonos.Kernel.Core.Scheduling
{
    public unsafe class Thread
    {
        public ThreadStatus Status = ThreadStatus.Empty;
        public Addr StackBottom;
        public Addr StackTop;
        //public IntPtr StackStatePointer;
        public IDTTaskStack* StackState;
        public Addr kernelStack = null;
        public Addr kernelStackBottom = null;
        public USize kernelStackSize = null;
        public uint Ticks;
        public bool User;
        public Process Process;
        public uint DataSelector;
        public uint ThreadID;
        public bool Debug;
        public string DebugName;
        public uint ArgumentBufferSize;

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
