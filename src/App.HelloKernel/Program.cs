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

        public static unsafe void Main()
        {
            ApplicationRuntime.Init();

            SetupDrivers();
            var port = Serial.COM2;
            Serial.SetupPort(port);

            var path = "test";
            var fileSize = (uint)GetFileLenth(path);
            var target = SysCalls.GetProcessIDForCommand(SysCallTarget.CreateMemoryProcess);
            var fileBuf = SysCalls.RequestMessageBuffer((uint)fileSize, target);
            var handle = OpenFile(path);
            var bufSize = 3000u;
            var buf = (byte*)RuntimeMemory.Allocate(bufSize);
            var gotBytes = (uint)ReadFile(handle, buf, bufSize);
            var fileBufPos = 0u;
            while (gotBytes > 0)
            {
                Console.WriteLine("got data");
                for (var i = 0; i < gotBytes; i++)
                    ((byte*)fileBuf.Start)[fileBufPos + i] = buf[i];
                fileBufPos += gotBytes;
                gotBytes = (uint)ReadFile(handle, buf, bufSize);
            }
            RuntimeMemory.Free(buf);
            SysCalls.CreateMemoryProcess(fileBuf, fileSize);
            //RuntimeMemory.Free(fileBuf);

            while (true)
            {
                //Serial.Write(port, (byte)'M');
            }
        }

        public unsafe struct MessageHeader
        {
            public int MsgId;
            public MessageCommand Command;
        }

        public enum MessageCommand
        {
            OpenFile = 240,
            WriteFile = 241,
            ReadFile = 242,
            GetFileLength = 243,
        }

        private static int lastMessageId = 0;
        public static unsafe FileHandle OpenFile(string path)
        {
            var msgId = ++lastMessageId;
            WriteHeader(new MessageHeader { Command = MessageCommand.OpenFile, MsgId = msgId });
            WriteArg(path);
            WriteEnd();
            return ReadResultInt32(msgId);
        }

        public static unsafe int GetFileLenth(string path)
        {
            var msgId = ++lastMessageId;
            WriteHeader(new MessageHeader { Command = MessageCommand.GetFileLength, MsgId = msgId });
            WriteArg(path);
            WriteEnd();
            return ReadResultInt32(msgId);
        }

        private static int ReadResultInt32(int msgId)
        {
            var lineType = ReadByte();
            Assert.True(lineType == (byte)LineType.Result);

            var msgId_ = ReadInt32();
            Assert.True(msgId_ == msgId);

            return ReadInt32();
        }

        public static unsafe int ReadFile(FileHandle handle, byte* buf, uint bufSize)
        {
            return -1;
        }

        public static unsafe int WriteFile(FileHandle handle, byte* buf, uint bufSize)
        {
            return -1;
        }

        public enum LineType : byte
        {
            Header = 200,
            Arg = 201,
            Data = 202,
            End = 203,
            Result = 204,
        }

        public static unsafe void WriteHeader(MessageHeader header)
        {
            var size = (uint)sizeof(MessageHeader);
            Write((byte)LineType.Header);
            var data = (byte*)(&header);
            for (var i = 0; i < size; i++)
                Write(data[i]);
        }

        public static unsafe void WriteData(uint bufSize, byte* data)
        {
            WriteDataStart(bufSize);
            for (var i = 0; i < bufSize; i++)
                WriteData(data[i]);
        }

        public static unsafe void WriteData(string data)
        {
            WriteDataStart((uint)data.Length);
            for (var i = 0; i < data.Length; i++)
                Write((byte)data[i]);
        }

        public static unsafe void WriteArg(string data)
        {
            WriteArgStart((uint)data.Length);
            for (var i = 0; i < data.Length; i++)
                Write((byte)data[i]);
        }

        public static unsafe void WriteEnd()
        {
            Write((byte)LineType.End);
        }

        private static unsafe void WriteDataStart(uint dataLength)
        {
            Write((byte)LineType.Data);
            Write(dataLength);
        }

        private static unsafe void WriteArgStart(uint argSize)
        {
            Write((byte)LineType.Arg);
            Write(argSize);
        }

        public static unsafe void WriteData(byte data)
        {
            Write(data);
        }

        private static unsafe void Write(uint data)
        {
            var bytes = BitConversion.GetBytes(data);
            Write(bytes);
            RuntimeMemory.FreeObject(bytes);
        }

        private static unsafe void Write(byte[] data)
        {
            for (var i = 0; i < data.Length; i++)
                Write(data[i]);
        }

        private static unsafe void Write(string data)
        {
            for (var i = 0; i < data.Length; i++)
                Write((byte)data[i]);
        }

        private static unsafe void Write(byte data)
        {
            Serial.Write(Serial.COM2, data);
        }

        private static byte ReadByte()
        {
            return Serial.Read(Serial.COM2);
        }

        private static int ReadInt32()
        {
            var buf = new byte[4];
            buf[0] = ReadByte();
            buf[1] = ReadByte();
            buf[2] = ReadByte();
            buf[3] = ReadByte();
            var result = BitConversion.GetInt32(buf);
            RuntimeMemory.FreeObject(buf);
            return result;
        }

        private static uint ReadUInt32()
        {
            return (uint)ReadInt32();
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
