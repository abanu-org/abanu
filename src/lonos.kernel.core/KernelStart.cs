// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using lonos.Kernel.Core.Api;
using lonos.Kernel.Core.Boot;
using lonos.Kernel.Core.Collections;
using lonos.Kernel.Core.Devices;
using lonos.Kernel.Core.Diagnostics;
using lonos.Kernel.Core.Elf;
using lonos.Kernel.Core.External;
using lonos.Kernel.Core.Interrupts;
using lonos.Kernel.Core.MemoryManagement;
using lonos.Kernel.Core.PageManagement;
using lonos.Kernel.Core.Processes;
using lonos.Kernel.Core.Scheduling;
using lonos.Kernel.Core.SysCalls;
using lonos.Kernel.Core.Tasks;
using Mosa.Runtime;
using Mosa.Runtime.x86;

namespace lonos.Kernel.Core
{

    public static class KernelStart
    {

        public static unsafe void Main()
        {
            ManagedMemoy.InitializeGCMemory();
            StartUp.InitializeAssembly();
            //Mosa.Runtime.StartUp.InitializeRuntimeMetadata();

            BootInfo.SetupStage1();

            Memory.InitialKernelProtect();

            ApiContext.Current = new ApiHost();
            Assert.Setup(AssertError);

            // Setup some pseudo devices
            DeviceManager.InitStage1();

            //Setup Output and Debug devices
            DeviceManager.InitStage2();

            // Write first output
            KernelMessage.WriteLine("<KERNEL:CONSOLE:BEGIN>");
            KernelMessage.WriteLine("Starting Lonos Kernel...");

            KernelMessage.WriteLine("KConfig.UseKernelMemoryProtection: {0}", KConfig.UseKernelMemoryProtection);
            KernelMessage.WriteLine("KConfig.UsePAE: {0}", KConfig.UsePAE);
            KernelMessage.WriteLine("Apply PageTableType: {0}", (uint)BootInfo.Header->PageTableType);

            Ulongtest1();
            Ulongtest2();

            // Detect environment (Memory Maps, Video Mode, etc.)
            BootInfo.SetupStage2();

            KernelMemoryMapManager.Setup();
            //KernelMemoryMapManager.Allocate(0x1000 * 1000, BootInfoMemoryType.PageDirectory);

            // Read own ELF-Headers and Sections
            KernelElf.Setup();

            // Initialize the embedded code (actually only a little proof of conecept code)
            NativeCalls.Setup();

            //InitialKernelProtect();

            PageFrameManager.Setup();

            KernelMessage.WriteLine("free: {0}", PageFrameManager.PagesAvailable);
            PageFrameManager.AllocatePages(PageFrameRequestFlags.Default, 10);
            KernelMessage.WriteLine("free: {0}", PageFrameManager.PagesAvailable);
            RawVirtualFrameAllocator.Setup();

            Memory.Setup();

            // Now Memory Sub System is working. At this point it's valid
            // to allocate memory dynamicly

            DeviceManager.InitFrameBuffer();

            // Setup Programmable Interrupt Table
            PIC.Setup();

            // Setup Interrupt Descriptor Table
            // Important Note: IDT depends on GDT. Never setup IDT before GDT.
            IDTManager.Setup();

            InitializeUserMode();
            SysCallManager.Setup();

            KernelMessage.WriteLine("Initialize Runtime Metadata");
            StartUp.InitializeRuntimeMetadata();

            KernelMessage.WriteLine("Performing some Non-Thread Tests");
            Tests();

            if (KConfig.SingleThread)
                StartupStage2();
            else
                ProcessManager.Setup(StartupStage2);
        }

        public static Service serv;

        public static void StartupStage2()
        {
            if (!KConfig.SingleThread)
            {
                Scheduler.CreateThread(ProcessManager.System, new ThreadStartOptions(BackgroundWorker.ThreadMain) { DebugName = "BackgroundWorker" }).Start();
                Scheduler.CreateThread(ProcessManager.System, new ThreadStartOptions(Thread0) { DebugName = "KernelThread0" }).Start();

                var userProc = ProcessManager.CreateEmptyProcess(new ProcessCreateOptions { User = true });
                userProc.Path = "/bulidin/testproc";
                Scheduler.CreateThread(userProc, new ThreadStartOptions(Thread1) { AllowUserModeIOPort = true, DebugName = "UserThread1" });
                Scheduler.CreateThread(userProc, new ThreadStartOptions(Thread2) { AllowUserModeIOPort = true, DebugName = "UserThread2" });
                userProc.Start();

                var proc = ProcessManager.StartProcess("app.HelloService");
                serv = new Service(proc);

                var p2 = ProcessManager.StartProcess("app.HelloKernel");
                //p2.Threads[0].SetArgument(0, 0x90);
                //p2.Threads[0].SetArgument(4, 0x94);
                //p2.Threads[0].SetArgument(8, 0x98);
                p2.Threads[0].Debug = true;

                //ProcessManager.System.Threads[0].Status = ThreadStatus.Terminated;
            }

            KernelMessage.WriteLine("Enter Main Loop");
            AppMain();
        }

        public static Addr tssAddr = null;
        //public static Addr kernelStack = null;
        //public static Addr kernelStackBottom = null;
        //public static USize kernelStackSize = null;

        public static void InitializeUserMode()
        {
            if (!KConfig.UseUserMode)
                return;

            if (KConfig.UseTaskStateSegment)
            {
                //kernelStackSize = 256 * 4096;
                tssAddr = RawVirtualFrameAllocator.RequestRawVirtalMemoryPages(1);
                MemoryManagement.PageTableExtensions.SetWritable(PageTable.KernelTable, tssAddr, 4096);
                //kernelStack = RawVirtualFrameAllocator.RequestRawVirtalMemoryPages(256); // TODO: Decrease Kernel Stack, because Stack have to be changed directly because of multi-threading.
                //kernelStackBottom = kernelStack + kernelStackSize;

                //KernelMessage.WriteLine("tssEntry: {0:X8}, tssKernelStack: {1:X8}-{2:X8}", tssAddr, kernelStack, kernelStackBottom - 1);

                //MemoryManagement.PageTableExtensions.SetWritable(PageTable.KernelTable, kernelStack, 256 * 4096);
            }
            GDT.SetupUserMode(tssAddr);
        }

        private static void Thread0()
        {
            KernelMessage.WriteLine("Thread0: Enter");
            uint i = 0;
            while (true)
            {
                i++;
                //if (Scheduler.ClockTicks % 100 == 0)
                Screen.Goto(3, 0);
                Screen.Write("TH_KERNEL:");
                Screen.Write(i, 10);
            }
            KernelMessage.WriteLine("Thread0: Finished");
        }

        private static void Thread1()
        {
            KernelMessage.WriteLine("Thread1: Enter");
            uint i = 0;
            while (true)
            {
                i++;
                //if (Scheduler.ClockTicks % 100 == 0)
                Screen.Goto(0, 0);
                Screen.Write("TH1:");
                Screen.Write(i, 10);
            }
            KernelMessage.WriteLine("Thread1: Finished");
        }

        private static void Thread2()
        {
            KernelMessage.WriteLine("Thread2: Enter");
            uint i = 0;
            while (i < 100)
            {
                i++;
                //if (Scheduler.ClockTicks % 100 == 0)
                Screen.Goto(1, 0);
                Screen.Write("TH2:");
                Screen.Write(i, 10);
            }
            KernelMessage.WriteLine("Thread2: Finished");
            //while (true)
            //    i++;
        }

        // public unsafe static void InitialKernelProtect()
        // {
        //     KernelMessage.WriteLine("Protecting Memory...");

        //     // PageDirectoryEntry* pde = (PageDirectoryEntry*)AddrPageDirectory;
        //     // for (int index = 0; index < 1024; index++)
        //     // {
        //     //   pde[index].Writable = false;
        //     // }

        //     // PageTable.PageTableEntry* pte = (PageTable.PageTableEntry*)PageTable.AddrPageTable;
        //     // for (int index = 0; index < 1024 * 32; index++)
        //     //   pte[index].Writable = false;

        //     // InitialKernelProtect_MakeWritable_ByRegion(0, 90 * 1024 * 1024);

        //     KernelMessage.WriteLine("Reload CR3 to {0:X8}", PageTable.AddrPageDirectory);
        //     Native.SetCR3(PageTable.AddrPageDirectory);
        //     //Native.Invlpg();
        //     KernelMessage.WriteLine("Protecting Memory done");
        // }

        // public unsafe static void InitialKernelProtect_MakeWritable_ByRegion(uint startVirtAddr, uint endVirtAddr)
        // {
        //     InitialKernelProtect_MakeWritable_BySize(startVirtAddr, endVirtAddr - startVirtAddr);
        // }

        // public unsafe static void InitialKernelProtect_MakeWritable_BySize(uint virtAddr, uint size)
        // {
        //     var pages = KMath.DivCeil(size, 4096);
        //     for (var i = 0; i < pages; i++)
        //     {
        //         var entry = PageTable.GetTableEntry(virtAddr);
        //         entry->Writable = true;
        //     }
        // }

        private static void Ulongtest1()
        {
            uint mask = 0x00004000;
            uint v1 = 0x00000007;
            uint r1 = v1.SetBits(12, 52, mask, 12); //52 with uint makes no sense, but this doesnt matter in this case, the result just works as expected. It works correct with count<32, too, of course.
                                                    // r1 =  00004007
            ulong v2 = v1;
            ulong r2 = v2.SetBits(12, 52, mask, 12);
            uint r2Int = (uint)r2;
            // r2Int = 00000007. This is wrong. It should be the same as r1.

            KernelMessage.WriteLine("bla1: {0:X8}", r1);
            KernelMessage.WriteLine("bla2: {0:X8}", r2Int);
        }

        private static unsafe void Ulongtest2()
        {
            ulong addr = 0x0000000019ad000;
            ulong data = 40004005;
            ulong result = data.SetBits(12, 52, addr, 12);

            var rAddr = (uint*)(void*)&result;
            var r1 = rAddr[0];
            var r2 = rAddr[1];

            KernelMessage.WriteLine("r1: {0:X8}", r1);
            KernelMessage.WriteLine("r2: {0:X8}", r2);
        }

        public static void Tests()
        {
            var ar = new KList<uint>();
            ar.Add(44);
            ar.Add(55);
            KernelMessage.WriteLine("CNT: {0}", ManagedMemoy.AllocationCount);
            foreach (var num in ar)
            {
                KernelMessage.WriteLine("VAL: {0}", num);
            }
            KernelMessage.WriteLine("CNT: {0}", ManagedMemoy.AllocationCount);
            ar.Destroy();

            KernelMessage.WriteLine("Pages free: {0}", PageFrameManager.PagesAvailable);

            for (var i = 0; i < 10000; i++)
            {
                var s = new int[] { 1, 2, 3, 4, };
                s[1] = 5;
                Memory.FreeObject(s);
            }
            KernelMessage.WriteLine("Pages free: {0}", PageFrameManager.PagesAvailable);
            //Memory.FreeObject(s);

        }

        public static void AppMain()
        {
            KernelMessage.WriteLine("Kernel ready");

            // We have nothing todo (yet). So let's stop.
            Debug.Break();
        }

        private static void Dummy()
        {
            //This is a dummy call, that get never executed.
            //Its requied, because we need a real reference to Mosa.Runtime.x86
            //Without that, the .NET compiler will optimize that reference away
            //if its nowhere used. Than the Compiler dosnt know about that Refernce
            //and the Compilation will fail
            Mosa.Runtime.x86.Internal.GetStackFrame(0);
        }

        public const uint Columns = 80;

        /// <summary>
        /// The rows
        /// </summary>
        public const uint Rows = 40;

        public static void RawWrite(uint row, uint column, char chr, byte color)
        {
            IntPtr address = new IntPtr(0x0B8000 + (((row * Columns) + column) * 2));

            Intrinsic.Store8(address, (byte)chr);
            Intrinsic.Store8(address, 1, color);
        }

        private static void AssertError(string message)
        {
            Panic.Error(message);
        }

    }
}
