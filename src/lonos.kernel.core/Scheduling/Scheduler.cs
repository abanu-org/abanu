// Adopted Implementation from: MOSA Project

using Mosa.Runtime;
using Mosa.Runtime.x86;
using System;
using System.Runtime.CompilerServices;
using System.Threading;

using System.Runtime.InteropServices;

namespace lonos.kernel.core
{
    public static class Scheduler
    {
        public const int MaxThreads = 256;
        public const int ClockIRQ = 0x20;
        public const int ThreadTerminationSignalIRQ = 254;

        private static bool Enabled;
        private static IntPtr SignalThreadTerminationMethodAddress;

        private static Thread[] Threads;

        private static uint CurrentThreadID;

        private static int clockTicks = 0;

        public static uint ClockTicks { get { return (uint)clockTicks; } }

        public static void Setup()
        {
            Enabled = false;
            Threads = new Thread[MaxThreads];
            CurrentThreadID = 0;
            clockTicks = 0;

            for (int i = 0; i < MaxThreads; i++)
            {
                Threads[i] = new Thread();
            }

            var address = GetAddress(IdleThread);

            SignalThreadTerminationMethodAddress = GetAddress(SignalThreadTerminationMethod);

            CreateThread(address, 0x4000, 0);
        }

        public static void Start()
        {
            SetThreadID(0);
            Enabled = true;

            //Native.Cli();
            Native.Int(ClockIRQ);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void IdleThread()
        {
            while (true)
            {
                Native.Hlt();
            }
        }

        public static void ClockInterrupt(IntPtr stackSate)
        {
            Interlocked.Increment(ref clockTicks);

            if (!Enabled)
                return;

            // Save current stack state
            var threadID = GetCurrentThreadID();
            SaveThreadState(threadID, stackSate);

            ScheduleNextThread(threadID);
        }

        private static void ScheduleNextThread(uint threadID)
        {
            threadID = GetNextThread(threadID);
            SwitchToThread(threadID);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void SignalThreadTerminationMethod()
        {
            KernelMessage.WriteLine("Terminating Thread");
            //Native.Int(ThreadTerminationSignalIRQ);
            TerminateCurrentThread();
        }

        public static void TerminateCurrentThread()
        {
            var threadID = GetCurrentThreadID();

            if (threadID != 0)
            {
                TerminateThread(threadID);
            }
        }

        private static void TerminateThread(uint threadID)
        {
            var thread = Threads[threadID];

            if (thread.Status == ThreadStatus.Running)
            {
                thread.Status = ThreadStatus.Terminating;

                // TODO: release stack memory
            }
            while (true)
            {
                Native.Hlt();
            }
        }

        private static uint GetNextThread(uint currentThreadID)
        {
            uint threadID = currentThreadID;

            if (currentThreadID == 0)
                currentThreadID = 1;

            while (true)
            {
                threadID++;

                if (threadID == MaxThreads)
                    threadID = 1;

                var thread = Threads[threadID];

                if (thread.Status == ThreadStatus.Running)
                    return threadID;

                if (currentThreadID == threadID)
                    return 0; // idle thread
            }
        }

        private static IntPtr GetAddress(ThreadStart d)
        {
            return Intrinsic.GetDelegateMethodAddress(d);
        }

        private static IntPtr GetAddress(ParameterizedThreadStart d)
        {
            return Intrinsic.GetDelegateTargetAddress(d);
        }

        public static uint CreateThread(ThreadStart thread, uint stackSize)
        {
            var address = GetAddress(thread);

            return CreateThread(address, stackSize);
        }

        public static uint CreateThread(IntPtr methodAddress, uint stackSize)
        {
            //Assert.True(stackSize != 0, "CreateThread(): invalid stack size = " + stackSize.ToString());
            //Assert.True(stackSize % PageFrameAllocator.PageSize == 0, "CreateThread(): invalid stack size % PageSize, stack size = " + stackSize.ToString());

            uint threadID = FindEmptyThreadSlot();

            if (threadID == 0)
            {
                ResetTerminatedThreads();
                threadID = FindEmptyThreadSlot();
            }

            //Assert.True(threadID != 0, "CreateThread(): invalid thread id = 0");

            CreateThread(methodAddress, stackSize, threadID);

            return threadID;
        }

        private unsafe static void CreateThread(IntPtr methodAddress, uint stackSize, uint threadID)
        {
            var thread = Threads[threadID];

            var stackPages = KMath.DivCeil(stackSize, PageFrameManager.PageSize);
            stackSize = stackPages * PageFrameManager.PageSize;
            KernelMessage.WriteLine("Create Thread. Stack size: {0:X8}", stackSize);
            var stack = new IntPtr((void*)RawVirtualFrameAllocator.RequestRawVirtalMemoryPages(stackPages));
            KernelMessage.WriteLine("Stack Addr: {0:X8}", (uint)stack);
            Memory.InitialKernelProtect_MakeWritable_BySize((uint)stack, stackSize);
            var stackTop = stack + (int)stackSize;

            var stackStateOffset = 2 * 4;

            Intrinsic.Store32(stackTop, -4, 0);          // Zero Sentinel
            Intrinsic.Store32(stackTop, -8, SignalThreadTerminationMethodAddress.ToInt32());  // Address of method that will raise a interrupt signal to terminate thread

            var stackState = (IDTStack*)(stackTop - IDTStack.Size - stackStateOffset);
            stackState[0] = new IDTStack();
            stackState->EFLAGS = 0x00000202;
            stackState->CS = 0x18;
            stackState->EIP = (uint)methodAddress.ToInt32();
            stackState->EBP = (uint)(stackTop - stackStateOffset).ToInt32();

            thread.Status = ThreadStatus.Running;
            thread.StackBottom = stack;
            thread.StackTop = stackTop;
            thread.StackStatePointer = stackTop - 60;
        }

        private static void SaveThreadState(uint threadID, IntPtr stackSate)
        {
            //Assert.True(threadID < MaxThreads, "SaveThreadState(): invalid thread id > max");

            var thread = Threads[threadID];

            //Assert.True(thread != null, "SaveThreadState(): thread id = null");

            thread.StackStatePointer = stackSate;
        }

        private static uint GetCurrentThreadID()
        {
            return CurrentThreadID;

            //return Native.GetFS();
        }

        private static void SetThreadID(uint threadID)
        {
            CurrentThreadID = threadID;

            //Native.SetFS(threadID);
        }

        private static void SwitchToThread(uint threadID)
        {
            var thread = Threads[threadID];

            //Assert.True(thread != null, "invalid thread id");

            thread.Ticks++;

            SetThreadID(threadID);

            PIC.SendEndOfInterrupt(ClockIRQ);
            //Native.InterruptReturn((uint)thread.StackStatePointer.ToInt32());
            InterruptReturn((uint)thread.StackStatePointer.ToInt32());
        }

        [DllImport("lonos.InterruptReturn.o", EntryPoint = "InterruptReturn")]
        private extern static void InterruptReturn(uint stackStatePointer);

        private static uint FindEmptyThreadSlot()
        {
            for (uint i = 0; i < MaxThreads; i++)
            {
                if (Threads[i].Status == ThreadStatus.Empty)
                    return i;
            }

            return 0;
        }

        private static void ResetTerminatedThreads()
        {
            for (uint i = 0; i < MaxThreads; i++)
            {
                if (Threads[i].Status == ThreadStatus.Terminated)
                {
                    Threads[i].Status = ThreadStatus.Empty;
                }
            }
        }
    }
}
