// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Lonos.Kernel.Core;
using Lonos.Runtime;
using Mosa.DeviceDriver;
using Mosa.DeviceDriver.ISA;
using Mosa.DeviceDriver.ScanCodeMap;
using Mosa.DeviceSystem;
using Mosa.DeviceSystem.PCI;
using Mosa.FileSystem.FAT;
using Mosa.Runtime.x86;

namespace Lonos.Kernel
{

    public static class Program
    {

        public static void Main()
        {
            ApplicationRuntime.Init();

            //var result = MessageManager.Send(SysCallTarget.ServiceFunc1, 55);
            SetupDrivers();

            while (true)
            {
            }
        }

        private static void SetupDrivers()
        {
            Console.WriteLine("> Initializing services...");

            // Create Service manager and basic services
            var serviceManager = new ServiceManager();

            var deviceService = new DeviceService();
            var diskDeviceService = new DiskDeviceService();
            var partitionService = new PartitionService();
            var pciControllerService = new PCIControllerService();
            var pciDeviceService = new PCIDeviceService();

            serviceManager.AddService(deviceService);
            serviceManager.AddService(diskDeviceService);
            serviceManager.AddService(partitionService);
            serviceManager.AddService(pciControllerService);
            serviceManager.AddService(pciDeviceService);

            Console.WriteLine("> Initializing hardware abstraction layer...");

            // Set device driver system with the hardware HAL
            var hardware = new Hardware();
            Mosa.DeviceSystem.Setup.Initialize(hardware, deviceService.ProcessInterrupt);

            Console.WriteLine("> Registering device drivers...");
            deviceService.RegisterDeviceDriver(Mosa.DeviceDriver.Setup.GetDeviceDriverRegistryEntries());

            Console.WriteLine("> Starting devices...");
            deviceService.Initialize(new X86System(), null);

            Console.Write("> Probing for ISA devices...");
            var isaDevices = deviceService.GetChildrenOf(deviceService.GetFirstDevice<ISABus>());
            Console.WriteLine("[Completed: " + isaDevices.Count.ToString() + " found]");

            foreach (var device in isaDevices)
            {
                Console.Write("  ");
                //Bullet(ScreenColor.Yellow);
                Console.Write(" ");
                //InBrackets(device.Name, ScreenColor.White, ScreenColor.Green);
                Console.Write(device.Name);
                Console.WriteLine();
            }

            Console.Write("> Probing for PCI devices...");
            var devices = deviceService.GetDevices<PCIDevice>();
            Console.WriteLine("[Completed: " + devices.Count.ToString() + " found]");

            foreach (var device in devices)
            {
                Console.Write("  ");
                //Bullet(ScreenColor.Yellow);
                Console.Write(" ");

                var pciDevice = device.DeviceDriver as PCIDevice;
                Console.Write(device.Name + ": " + pciDevice.VendorID.ToString("x") + ":" + pciDevice.DeviceID.ToString("x") + " " + pciDevice.SubSystemID.ToString("x") + ":" + pciDevice.SubSystemVendorID.ToString("x") + " (" + pciDevice.Function.ToString("x") + ":" + pciDevice.ClassCode.ToString("x") + ":" + pciDevice.SubClassCode.ToString("x") + ":" + pciDevice.ProgIF.ToString("x") + ":" + pciDevice.RevisionID.ToString("x") + ")");

                var children = deviceService.GetChildrenOf(device);

                if (children.Count != 0)
                {
                    var child = children[0];

                    Console.WriteLine();
                    Console.Write("    ");

                    var pciDevice2 = child.DeviceDriver as PCIDevice;
                    Console.Write(child.Name);
                }

                Console.WriteLine();
            }

            Console.Write("> Probing for disk controllers...");
            var diskcontrollers = deviceService.GetDevices<IDiskControllerDevice>();
            Console.WriteLine("[Completed: " + diskcontrollers.Count.ToString() + " found]");

            foreach (var device in diskcontrollers)
            {
                Console.Write("  ");
                //Bullet(ScreenColor.Yellow);
                Console.Write(" ");
                Console.Write(device.Name);
                Console.WriteLine();
            }

            Console.Write("> Probing for disks...");
            var disks = deviceService.GetDevices<IDiskDevice>();
            Console.WriteLine("[Completed: " + disks.Count.ToString() + " found]");

            foreach (var disk in disks)
            {
                Console.Write("  ");
                Console.Write(" ");
                Console.Write(disk.Name);
                Console.Write(" " + (disk.DeviceDriver as IDiskDevice).TotalBlocks.ToString() + " blocks");
                Console.WriteLine();
            }

            partitionService.CreatePartitionDevices();

            Console.Write("> Finding partitions...");
            var partitions = deviceService.GetDevices<IPartitionDevice>();
            Console.WriteLine("[Completed: " + partitions.Count.ToString() + " found]");

            //foreach (var partition in partitions)
            //{
            //  Console.Write("  ");
            //  Bullet(ScreenColor.Yellow);
            //  Console.Write(" ");
            //  InBrackets(partition.Name, ScreenColor.White, ScreenColor.Green);
            //  Console.Write(" " + (partition.DeviceDriver as IPartitionDevice).BlockCount.ToString() + " blocks");
            //  Console.WriteLine();
            //}

            Console.Write("> Finding file systems...");

            foreach (var partition in partitions)
            {
                var fat = new FatFileSystem(partition.DeviceDriver as IPartitionDevice);

                if (fat.IsValid)
                {
                    Console.WriteLine("Found a FAT file system!");

                    const string filename = "TEST.TXT";

                    var location = fat.FindEntry(filename);

                    if (location.IsValid)
                    {
                        Console.Write("Found: " + filename);

                        var fatFileStream = new FatFileStream(fat, location);

                        uint len = (uint)fatFileStream.Length;

                        Console.WriteLine(" - Length: " + len.ToString());

                        Console.Write("Reading File: ");

                        while (true)
                        {
                            int i = fatFileStream.ReadByte();

                            if (i < 0)
                                break;

                            Console.Write((char)i);
                        }

                        Console.WriteLine();
                    }
                }
            }

            // Get StandardKeyboard
            var standardKeyboards = deviceService.GetDevices("StandardKeyboard");

            if (standardKeyboards.Count == 0)
            {
                Console.WriteLine("No Keyboard!");
                //ForeverLoop();
            }

            var standardKeyboard = standardKeyboards[0].DeviceDriver as IKeyboardDevice;

            //Debug = ConsoleManager.Controller.Debug;

            // setup keymap
            var keymap = new US();

            // setup keyboard (state machine)
            var keyboard = new Mosa.DeviceSystem.Keyboard(standardKeyboard, keymap);

        }

    }

    public static class Console
    {

        private static MemoryRegion buf;

        static Console()
        {
            var writeDebugMessageProcID = SysCalls.GetProcessIDForCommand(SysCallTarget.WriteDebugMessage);
            buf = SysCalls.RequestMessageBuffer(4096, writeDebugMessageProcID);
        }

        public static void Write(string msg)
        {
            SysCalls.WriteDebugMessage(buf, msg);
        }

        public static void WriteLine(string msg)
        {
            SysCalls.WriteDebugMessage(buf, msg);
            SysCalls.WriteDebugChar('\n');
        }

        public static void WriteLine()
        {
            SysCalls.WriteDebugChar('\n');
        }

        public static void Write(char c)
        {
            SysCalls.WriteDebugChar(c);
        }
    }

}
