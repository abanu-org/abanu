// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using Abanu.Kernel.Core.Boot;
using Abanu.Kernel.Core.Diagnostics;
using Abanu.Kernel.Core.MemoryManagement;
using Abanu.Kernel.Core.PageManagement;
using Mosa.Runtime;

namespace Abanu.Kernel.Core.Devices
{

    public static class DeviceManager
    {

        public static IBuffer Serial1;

        private static IBuffer BiosTextScreen;
        private static IBuffer FrameBufferTextScreen;
        public static ConsoleDevice Console;

        public static IBuffer Null;
        public static IBuffer KMsg;

        internal static FrameBuffer Fb;

        /// <summary>
        /// Pseudo devices
        /// </summary>
        public static void InitStage1()
        {
            Null = new NullDevice();
            KMsg = new KernelMessageDevice();
            KernelMessage.SetHandler(KMsg);
        }

        /// <summary>
        /// Output and Debug devices
        /// </summary>
        public static unsafe void InitStage2()
        {
            Serial.SetupPort(Serial.COM1);
            Serial1 = new SerialDevice(Serial.COM1);

            PageTable.KernelTable.SetWritable(Screen.ScreenMemoryAddress, Screen.ScreenMemorySize);
            Screen.EarlyInitialization();
            BiosTextScreen = new BiosTextScreenDevice();

            Screen.ApplyMode(BootInfo.Header->VBEMode);

            Console = new ConsoleDevice(BiosTextScreen);
        }

        /// <summary>
        /// Video Stage
        /// </summary>
        public static unsafe void InitFrameBuffer()
        {
            if (!BootInfo.Header->FBPresent || BootInfo.Header->VBEMode < 0x100)
            {
                KernelMessage.Path("fb", "not present");
                return;
            }

            KernelMessage.WriteLine("InitFrameBuffer");

            Fb = new FrameBuffer(ref BootInfo.Header->FbInfo);
            for (uint at = Fb.Addr; at < Fb.Addr + Fb.MemorySize; at += 4096)
                PageTable.KernelTable.MapVirtualAddressToPhysical(at, at);
            PageTable.KernelTable.Flush();

            FrameBufferTextScreen = new FrameBufferTextScreenDevice(Fb);
            Console.SetOutputDevice(FrameBufferTextScreen);
        }

        public static IBuffer GetDevice(string devName)
        {
            switch (devName)
            {
                case "/dev/ttyS0":
                    return Serial1;
                case "/dev/console":
                    return Console;
                case "/dev/null":
                    return Null;
                case "/dev/kmsg":
                    return KMsg;
                default:
                    return null;
            }
        }

    }
}
