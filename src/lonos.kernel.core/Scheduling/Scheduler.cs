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

            SignalThreadTerminationMethodAddress = GetAddress(SignalThreadTerminationMethod);

            CreateThread(new KThreadStartOptions(IdleThread), 0);

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

            // Normally, you should never go here
            Panic.Error("Main-Thread still alive");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void IdleThread()
        {
            while (true)
            {
                //Native.Hlt();
                //Native.Nop();
                uint i = 0;
                while (true)
                {
                    i++;
                    Screen.Goto(2, 0);
                    Screen.Write("IDLE:");
                    Screen.Write(i, 10);
                }
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

        private static void ScheduleNextThread(uint currentThreadID)
        {
            var nextThreadID = GetNextThread(currentThreadID);
            SwitchToThread(nextThreadID, currentThreadID);
        }

        private static void ThreadEntry(uint threadID)
        {
            var thread = Threads[threadID];
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void SignalThreadTerminationMethod()
        {
            //KernelMessage.WriteLine("Terminating Thread");
            //Native.Int(ThreadTerminationSignalIRQ);
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

                if (thread.Status == ThreadStatus.Running || thread.Status == ThreadStatus.Creating)
                    return threadID;

                if (currentThreadID == threadID)
                    return 0; // idle thread
            }
        }

        private static IntPtr GetAddress(ThreadStart d)
        {
            return Intrinsic.GetDelegateMethodAddress(d);
        }

        public static uint CreateThread(KThreadStartOptions options)
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

            CreateThread(options, threadID);

            return threadID;
        }

        private unsafe static void CreateThread(KThreadStartOptions options, uint threadID)
        {
            var thread = Threads[threadID];

            // Debug:
            options.User = true;

            thread.User = options.User;

            var stackSize = options.StackSize;

            var stackPages = KMath.DivCeil(stackSize, PageFrameManager.PageSize);
            stackSize = stackPages * PageFrameManager.PageSize;
            KernelMessage.WriteLine("Create Thread. Stack size: {0:X8}", stackSize);
            var stack = new IntPtr((void*)RawVirtualFrameAllocator.RequestRawVirtalMemoryPages(stackPages));
            KernelMessage.WriteLine("Stack Addr: {0:X8}. EntryPoint: {1:X8}", (uint)stack, options.MethodAddr);
            Memory.InitialKernelProtect_MakeWritable_BySize((uint)stack, stackSize);
            var stackBottom = stack + (int)stackSize;

            var stackStateOffset = 8;
            if (options.User)
                stackStateOffset += 8;

            uint CS = 0x08;
            if (options.User)
                CS = 0x1B;

            thread.Status = ThreadStatus.Creating;
            thread.StackTop = stack;
            thread.StackBottom = stackBottom;
            thread.StackStatePointer = stackBottom - IDTStack.Size - stackStateOffset;

            Intrinsic.Store32(stackBottom, -4, 0);          // Zero Sentinel
            Intrinsic.Store32(stackBottom, -8, SignalThreadTerminationMethodAddress.ToInt32());  // Address of method that will raise a interrupt signal to terminate thread
            if (options.User)
            {
                Intrinsic.Store32(stackBottom, -12, 0x23);
                Intrinsic.Store32(stackBottom, -16, (uint)thread.StackStatePointer);
            }

            var stackState = (IDTStack*)(stackBottom - IDTStack.Size - stackStateOffset);
            stackState[0] = new IDTStack();
            stackState->EFLAGS = 0x00000202;
            if (options.User && options.AllowUserModeIOPort)
            {
                byte IOPL = 3;
                stackState->EFLAGS = stackState->EFLAGS.SetBits(12, 2, IOPL);
            }
            stackState->CS = CS;
            stackState->EIP = options.MethodAddr;
            stackState->EBP = (uint)(stackBottom - stackStateOffset).ToInt32();
        }

        private static void SaveThreadState(uint threadID, IntPtr stackSate)
        {
            //Assert.True(threadID < MaxThreads, "SaveThreadState(): invalid thread id > max");

            var thread = Threads[threadID];

            if (thread.Status == ThreadStatus.Creating)
                return; // New threads doesn't have a stack in use. Take the initial one.

            //Assert.True(thread != null, "SaveThreadState(): thread id = null");

            if (KConfig.TraceThreadSwitch)
                KernelMessage.WriteLine("Thread {0}: Save StackPointer {1:X8}", threadID, (uint)stackSate);

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

        private static void SwitchToThread(uint threadID, uint oldThreadID)
        {
            var thread = Threads[threadID];
            var oldThread = Threads[oldThreadID];

            if (KConfig.TraceThreadSwitch)
                KernelMessage.WriteLine("Switching to Thread {0}. ThreadStack: {1:X8}", threadID, (uint)thread.StackStatePointer);

            var oldThreadWasUserMode = oldThread.User;

            //Assert.True(thread != null, "invalid thread id");

            thread.Ticks++;

            SetThreadID(threadID);

            PIC.SendEndOfInterrupt(ClockIRQ);

            if (thread.Status == ThreadStatus.Creating)
            {
                thread.Status = ThreadStatus.Running;
            }

            // K=Kernel, U=User, 1=Extra SS:ESP on Stack
            // K->K: 0->0: InterruptReturnKernelToKernel
            // U->U: 1->1: InterruptReturnUserToUser
            // U->K: 1->0: InterruptReturnUserToKernel
            // K->U: 0->1: InterruptReturnKernelToUser

            if (thread.User && oldThreadWasUserMode)
            {
                thread.Status = ThreadStatus.Running;
                InterruptReturnUser((uint)thread.StackStatePointer.ToInt32());
            }
            else
            {
                InterruptReturn((uint)thread.StackStatePointer.ToInt32());
                //Native.InterruptReturn((uint)thread.StackStatePointer.ToInt32());
            }
        }

        [DllImport("lonos.InterruptReturn.o", EntryPoint = "InterruptReturn")]
        private extern static void InterruptReturn(uint stackStatePointer);

        [DllImport("lonos.InterruptReturnUser.o", EntryPoint = "InterruptReturnUser")]
        private extern static void InterruptReturnUser(uint stackStatePointer);

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

    //public struct LocalThreadStartInfo
    //{
    //    public uint MagicValue1;
    //    public uint ThreadID;
    //    public bool RequestAbort;
    //    public uint MagicValue2;
    //}

    public struct KThreadStartOptions
    {
        public Addr MethodAddr;
        public bool User;
        public uint StackSize;
        public bool AllowUserModeIOPort;

        public KThreadStartOptions(ThreadStart start)
        {
            MethodAddr = Intrinsic.GetDelegateMethodAddress(start);
            User = false;
            AllowUserModeIOPort = KConfig.AllowUserModeIOPort;
            StackSize = KConfig.DefaultStackSize;
        }

        public KThreadStartOptions(Addr methodAddr)
        {
            MethodAddr = methodAddr;
            User = false;
            AllowUserModeIOPort = KConfig.AllowUserModeIOPort;
            StackSize = KConfig.DefaultStackSize;
        }

    }

}
