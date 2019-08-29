// Adopted Implementation from: MOSA Project

using Mosa.Runtime;
using Mosa.Runtime.x86;
using System;
using System.Runtime.CompilerServices;
using System.Threading;

using System.Runtime.InteropServices;
using lonos.Kernel.Core.Interrupts;
using lonos.Kernel.Core.MemoryManagement;
using lonos.Kernel.Core.Diagnostics;

namespace lonos.Kernel.Core.Scheduling
{
    public static class Scheduler
    {
        public const int ThreadCapacity = 10;
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
            Threads = new Thread[ThreadCapacity];
            CurrentThreadID = 0;
            clockTicks = 0;

            for (int i = 0; i < ThreadCapacity; i++)
            {
                Threads[i] = new Thread();
            }

            SignalThreadTerminationMethodAddress = GetAddress(SignalThreadTerminationMethod);

            CreateThread(new ThreadStartOptions(IdleThread));

            //Debug, for breakpoint
            //clockTicks++;

            //AsmDebugFunction.DebugFunction1();
        }

        public static void Start()
        {
            SetThreadID(0);
            Enabled = true;

            //Native.Cli();
            Native.Int(ClockIRQ);

            // Normally, you should never get here
            Panic.Error("Main-Thread still alive");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void IdleThread()
        {
            while (true)
                Native.Hlt();
        }

        private static void dummy(uint local)
        {
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

        private static void ScheduleNextThread(uint currentThreadID)
        {
            var nextThreadID = GetNextThread(currentThreadID);
            SwitchToThread(nextThreadID);
        }

        private static void ThreadEntry(uint threadID)
        {
            var thread = Threads[threadID];
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void SignalThreadTerminationMethod()
        {
            // No local variables are allowd, because the stack could be totally empty.
            // Just make a call
            TerminateCurrentThread();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void TerminateCurrentThread()
        {
            KernelMessage.WriteLine("Terminating Thread");

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
                thread.Status = ThreadStatus.Terminated;

            // We are scheduled now for termination
            while (true)
                Native.Nop();
        }

        private static uint GetNextThread(uint currentThreadID)
        {
            uint threadID = currentThreadID;

            if (currentThreadID == 0)
                currentThreadID = 1;

            while (true)
            {
                threadID++;

                if (threadID == ThreadCapacity)
                    threadID = 1;

                var thread = Threads[threadID];

                if (thread.Status == ThreadStatus.Running || thread.Status == ThreadStatus.Created)
                    return threadID;

                if (currentThreadID == threadID)
                    return 0; // idle thread
            }
        }

        private static IntPtr GetAddress(ThreadStart d)
        {
            return Intrinsic.GetDelegateMethodAddress(d);
        }

        private static object SyncRoot = new object();

        public unsafe static void CreateThread(ThreadStartOptions options)
        {
            Thread thread;
            uint threadID;
            lock (SyncRoot)
            {
                threadID = FindEmptyThreadSlot();

                if (threadID == 0)
                {
                    ResetTerminatedThreads();
                    threadID = FindEmptyThreadSlot();
                }

                thread = Threads[threadID];
                thread.Status = ThreadStatus.Creating;
            }

            // Debug:
            //options.User = false;

            thread.User = options.User;

            var stackSize = options.StackSize;

            var stackPages = KMath.DivCeil(stackSize, PageFrameManager.PageSize);
            var debugPadding = 8u;
            stackSize = stackPages * PageFrameManager.PageSize;
            var stack = new IntPtr((void*)RawVirtualFrameAllocator.RequestRawVirtalMemoryPages(stackPages));
            Memory.InitialKernelProtect_MakeWritable_BySize((uint)stack, stackSize);
            stackSize -= debugPadding;
            var stackBottom = stack + (int)stackSize;

            KernelMessage.Write("Create Thread {0}. EntryPoint: {1:X8} Stack: {2:X8}-{3:X8} Type: ", threadID, options.MethodAddr, (uint)stack, (uint)stackBottom - 1);
            if (thread.User)
                KernelMessage.WriteLine("User");
            else
                KernelMessage.WriteLine("Kernel");

            if (options.User)
                KernelMessage.WriteLine("StackState at {0:X8}", (uint)thread.StackState);

            var stackStateOffset = 8;

            uint CS = 0x08;
            if (options.User)
                CS = 0x1B;

            var stateSize = options.User ? IDTTaskStack.Size : IDTStack.Size;

            thread.StackTop = stack;
            thread.StackBottom = stackBottom;

            Intrinsic.Store32(stackBottom, 4, 0xFF00001);          // Debug Marker
            Intrinsic.Store32(stackBottom, 0, 0xFF00002);          // Debug Marker

            Intrinsic.Store32(stackBottom, -4, (uint)stackBottom);
            Intrinsic.Store32(stackBottom, -8, SignalThreadTerminationMethodAddress.ToInt32());  // Address of method that will raise a interrupt signal to terminate thread

            IDTTaskStack* stackState = null;
            if (thread.User)
                stackState = (IDTTaskStack*)Memory.Allocate(IDTTaskStack.Size);
            else
                stackState = (IDTTaskStack*)(stackBottom - 8 - IDTStack.Size); // IDTStackSize ist correct - we don't need the Task-Members.
            thread.StackState = stackState;

            stackState->Stack.EFLAGS = X86_EFlags.Reserved1;
            if (options.User)
            {
                // Never set this values for Non-User, otherwiese you will override stack informations.
                stackState->TASK_SS = 0x23;
                stackState->TASK_ESP = (uint)stackBottom - 8;
            }
            if (options.User && options.AllowUserModeIOPort)
            {
                byte IOPL = 3;
                stackState->Stack.EFLAGS = (X86_EFlags)((uint)stackState->Stack.EFLAGS).SetBits(12, 2, IOPL);
            }

            stackState->Stack.CS = CS;
            stackState->Stack.EIP = options.MethodAddr;
            stackState->Stack.EBP = (uint)(stackBottom - stackStateOffset).ToInt32();

            thread.Status = ThreadStatus.Created;
        }

        private unsafe static void SaveThreadState(uint threadID, IntPtr stackState)
        {
            //Assert.True(threadID < MaxThreads, "SaveThreadState(): invalid thread id > max");

            var thread = Threads[threadID];

            if (thread.Status != ThreadStatus.Running)
                return; // New threads doesn't have a stack in use. Take the initial one.

            //Assert.True(thread != null, "SaveThreadState(): thread id = null");

            if (thread.User)
            {
                Assert.IsSet(thread.StackState, "thread.StackState is null");
                *(thread.StackState) = *((IDTTaskStack*)stackState);
            }
            else
            {
                thread.StackState = (IDTTaskStack*)stackState;
            }

            if (KConfig.TraceThreadSwitch)
                KernelMessage.WriteLine("Task {0}: Stored ThreadState from {1:X8} stored at {2:X8}, ESP={3:X8}, EIP={4:X8}", threadID, (uint)stackState, (uint)thread.StackState, thread.StackState->TASK_ESP, thread.StackState->Stack.EIP);
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

        private unsafe static void SwitchToThread(uint threadID)
        {
            var thread = Threads[threadID];

            if (KConfig.TraceThreadSwitch)
                KernelMessage.WriteLine("Switching to Thread {0}. ThreadStack: {1:X8}", threadID, (uint)thread.StackState->TASK_ESP);

            //Assert.True(thread != null, "invalid thread id");

            thread.Ticks++;

            SetThreadID(threadID);

            PIC.SendEndOfInterrupt(ClockIRQ);

            if (thread.Status == ThreadStatus.Created)
                thread.Status = ThreadStatus.Running;

            thread.StackState->Stack.EFLAGS |= X86_EFlags.InterruptEnableFlag;

            uint stackStateAddr = (uint)thread.StackState;

            if (!thread.User)
                thread.StackState = null; // just to be sure

            InterruptReturn(stackStateAddr, thread.DataSelector);
        }

        [DllImport("x86/lonos.InterruptReturn.o", EntryPoint = "InterruptReturn")]
        private extern static void InterruptReturn(uint stackStatePointer, uint dataSegment);

        private static uint FindEmptyThreadSlot()
        {
            for (uint i = 0; i < ThreadCapacity; i++)
            {
                if (Threads[i].Status == ThreadStatus.Empty)
                    return i;
            }

            return 0;
        }

        public static void ResetTerminatedThreads()
        {
            lock (Threads)
            {
                for (uint i = 0; i < ThreadCapacity; i++)
                {
                    if (Threads[i].Status == ThreadStatus.Terminated)
                    {
                        ResetThread(i);
                    }
                }
            }
        }

        private unsafe static void ResetThread(uint threadID)
        {
            var thread = Threads[threadID];
            lock (thread)
            {
                if (thread.Status != ThreadStatus.Terminated)
                    return;

                thread.FreeMemory();
                KernelMessage.WriteLine("Thread disposed");

                thread.Status = ThreadStatus.Empty;
            }
        }

    }

}
