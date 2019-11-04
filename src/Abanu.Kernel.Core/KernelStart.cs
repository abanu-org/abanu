// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using Abanu.Kernel.Core.Api;
using Abanu.Kernel.Core.Boot;
using Abanu.Kernel.Core.Collections;
using Abanu.Kernel.Core.Devices;
using Abanu.Kernel.Core.Diagnostics;
using Abanu.Kernel.Core.Elf;
using Abanu.Kernel.Core.External;
using Abanu.Kernel.Core.Interrupts;
using Abanu.Kernel.Core.MemoryManagement;
using Abanu.Kernel.Core.PageManagement;
using Abanu.Kernel.Core.Processes;
using Abanu.Kernel.Core.Scheduling;
using Abanu.Kernel.Core.SysCalls;
using Abanu.Kernel.Core.Tasks;
using Mosa.Runtime;
using Mosa.Runtime.x86;

namespace Abanu.Kernel.Core
{

    public static class KernelStart
    {

        public static unsafe void Main()
        {
            try
            {

                ManagedMemoy.InitializeGCMemory();
                StartUp.InitializeAssembly();
                KMath.Init();
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
                PerformanceCounter.Setup(BootInfo.Header->KernelBootStartCycles);
                KernelMessage.WriteLine("Starting Abanu Kernel...");

                KernelMessage.WriteLine("KConfig.UseKernelMemoryProtection: {0}", KConfig.UseKernelMemoryProtection);
                KernelMessage.WriteLine("KConfig.UsePAE: {0}", KConfig.UsePAE);
                KernelMessage.WriteLine("Apply PageTableType: {0}", (uint)BootInfo.Header->PageTableType);
                KernelMessage.WriteLine("GCInitialMemory: {0:X8}-{1:X8}", Address.GCInitialMemory, Address.GCInitialMemory + Address.GCInitialMemorySize - 1);

                CodeTests.Run();

                // Detect environment (Memory Maps, Video Mode, etc.)
                BootInfo.SetupStage2();

                KernelMemoryMapManager.Setup();
                //KernelMemoryMapManager.Allocate(0x1000 * 1000, BootInfoMemoryType.PageDirectory);

                // Read own ELF-Headers and Sections
                KernelElf.Setup();

                // Initialize the embedded code (actually only a little proof of concept code)
                NativeCalls.Setup();

                //InitialKernelProtect();

                PhysicalPageManager.Setup();

                KernelMessage.WriteLine("Phys free: {0}", PhysicalPageManager.FreePages);
                PhysicalPageManager.AllocatePages(10);
                KernelMessage.WriteLine("Phys free: {0}", PhysicalPageManager.FreePages);
                VirtualPageManager.Setup();

                Memory.Setup();

                // Now Memory Sub System is working. At this point it's valid
                // to allocate memory dynamically

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
                NonThreadTests.RunTests();
            }
            catch (Exception ex)
            {
                Panic.Error(ex.Message);
            }

            if (KConfig.SingleThread)
                StartupStage2();
            else
                ProcessManager.Setup(StartupStage2);
        }

        public static Service Serv;
        public static Service FileServ;
        public static Service ServHostCommunication;

        public static unsafe void StartupStage2()
        {
            try
            {
                if (!KConfig.SingleThread)
                {
                    Scheduler.CreateThread(ProcessManager.System, new ThreadStartOptions(BackgroundWorker.ThreadMain) { DebugName = "BackgroundWorker", Priority = -5 }).Start();

                    ThreadTests.StartTestThreads();

                    var fileProc = ProcessManager.StartProcess("Service.Basic");
                    FileServ = fileProc.Service;

                    KernelMessage.WriteLine("Waiting for Service");
                    while (FileServ.Status != ServiceStatus.Ready)
                    {
                        Scheduler.Sleep(0);
                    }
                    KernelMessage.WriteLine("Service Ready");

                    var conProc = ProcessManager.StartProcess("Service.ConsoleServer");
                    var conServ = conProc.Service;
                    KernelMessage.WriteLine("Waiting for ConsoleServer");
                    while (conServ.Status != ServiceStatus.Ready)
                    {
                        Scheduler.Sleep(0);
                    }
                    KernelMessage.WriteLine("ConsoleServer Ready");

                    //var buf = Abanu.Runtime.SysCalls.RequestMessageBuffer(4096, FileServ.Process.ProcessID);
                    //var kb = Abanu.Runtime.SysCalls.OpenFile(buf, "/dev/keyboard");
                    //KernelMessage.Write("kb Handle: {0:X8}", kb);
                    //buf.Size = 4;
                    //Abanu.Runtime.SysCalls.WriteFile(kb, buf);
                    //Abanu.Runtime.SysCalls.ReadFile(kb, buf);

                    //var procHostCommunication = ProcessManager.StartProcess("Service.HostCommunication");
                    //ServHostCommunication = new Service(procHostCommunication);
                    //// TODO: Optimize Registration
                    //SysCallManager.SetCommandProcess(SysCallTarget.HostCommunication_CreateProcess, procHostCommunication);

                    var proc = ProcessManager.StartProcess("App.HelloService");
                    Serv = proc.Service;

                    var p2 = ProcessManager.StartProcess("App.HelloKernel");
                    //p2.Threads[0].SetArgument(0, 0x90);
                    //p2.Threads[0].SetArgument(4, 0x94);
                    //p2.Threads[0].SetArgument(8, 0x98);
                    p2.Threads[0].Debug = true;

                    var p3 = ProcessManager.StartProcess("App.Shell");

                    ProcessManager.System.Threads[0].Status = ThreadStatus.Terminated;
                }
                VirtualPageManager.SetTraceOptions(new PageFrameAllocatorTraceOptions { Enabled = true, MinPages = 1 });

                KernelMessage.WriteLine("Enter Main Loop");
                AppMain();
            }
            catch (Exception ex)
            {
                Panic.Error(ex.Message);
            }
        }

        public static Addr TssAddr = null;
        //public static Addr kernelStack = null;
        //public static Addr kernelStackBottom = null;
        //public static USize kernelStackSize = null;

        public static unsafe void InitializeUserMode()
        {
            if (!KConfig.UseUserMode)
                return;

            if (KConfig.UseTaskStateSegment)
            {
                //kernelStackSize = 256 * 4096;
                TssAddr = VirtualPageManager.AllocatePages(1);
                PageTable.KernelTable.SetWritable(TssAddr, 4096);
                KernelMemoryMapManager.Header->Used.Add(new KernelMemoryMap(TssAddr, 4096, BootInfoMemoryType.TSS, AddressSpaceKind.Virtual));
                //kernelStack = RawVirtualFrameAllocator.RequestRawVirtalMemoryPages(256); // TODO: Decrease Kernel Stack, because Stack have to be changed directly because of multi-threading.
                //kernelStackBottom = kernelStack + kernelStackSize;

                //KernelMessage.WriteLine("tssEntry: {0:X8}, tssKernelStack: {1:X8}-{2:X8}", tssAddr, kernelStack, kernelStackBottom - 1);

                //MemoryManagement.PageTableExtensions.SetWritable(PageTable.KernelTable, kernelStack, 256 * 4096);
            }

            // Disabling Interrupts here is very important, otherwise we will get randomly an Invalid TSS Exception.
            IDTManager.Stop();
            GDT.SetupUserMode(TssAddr);
            IDTManager.Start();
        }

        public static void AppMain()
        {
            KernelMessage.WriteLine("Kernel ready");

            TriggerTestPassed();

            // We have nothing to do (yet). So let's stop.
            Debug.Break();
        }

        /// <summary>
        /// Signal the Integration tests, that Test was successful.
        /// </summary>
        private static void TriggerTestPassed()
        {
            Uninterruptible.Execute(() => KernelMessage.WriteLine(KConfig.SelfTestPassedMarker));
        }

        private static void Dummy()
        {
            // This is a dummy call, that get never executed.
            // Its required, because we need a real reference to Mosa.Runtime.x86
            // Without that, the .NET compiler will optimize that reference away
            // if its nowhere used. Than the Compiler doesn't know about that reference
            // and the Compilation will fail
            Mosa.Runtime.x86.Internal.ExceptionHandler();
        }

        public const uint Columns = 80;

        /// <summary>
        /// The rows
        /// </summary>
        public const uint Rows = 40;

        public static void RawWrite(uint row, uint column, char chr, byte color)
        {
            Pointer address = new Pointer(0x0B8000 + (((row * Columns) + column) * 2));

            Intrinsic.Store8(address, (byte)chr);
            Intrinsic.Store8(address, 1, color);
        }

        private static void AssertError(string message, uint arg1 = 0, uint arg2 = 0, uint arg3 = 0)
        {
            var sb = new StringBuffer();
            sb.Append(message, arg1, arg2, arg3);
            Panic.Error(sb.CreateString());
        }

    }

}
