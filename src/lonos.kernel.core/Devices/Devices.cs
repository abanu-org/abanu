using System;
using Mosa.Runtime;
using Mosa.Kernel.x86;


namespace lonos.kernel.core
{

    public static class Devices
    {

        public static IFile Serial1;
        private static IFile BiosTextScreen;
        private static IFile FrameBufferTextScreen;
        public static ConsoleDevice Console;
        public static IFile Null;
        public static IFile KMsg;

        internal static FrameBuffer fb;

        /// <summary>
        /// Pseudeo devices 
        /// </summary>
        public static void InitStage1()
        {
            Null = new NullDevice();
            KMsg = new KernelMessageDevice();
        }

        /// <summary>
        /// Output and Debug devices
        /// </summary>
        public static void InitStage2()
        {
            Serial.SetupPort(Serial.COM1);
            Serial1 = new SerialDevice(Serial.COM1);

            lonos.kernel.core.Screen.EarlyInitialization();
            BiosTextScreen = new BiosTextScreenDevice();

            Console = new ConsoleDevice(BiosTextScreen);
        }

        /// <summary>
        /// Video Stage
        /// </summary>
        public unsafe static void InitFrameBuffer()
        {
            if (!Multiboot.VBEPresent)
                return;

            fb = new FrameBuffer(Multiboot.multiBootInfo->FbAddr, Multiboot.multiBootInfo->FbWidth, Multiboot.multiBootInfo->FbHeight, Multiboot.multiBootInfo->FbPitch, 8);
            fb.Init();

            FrameBufferTextScreen = new FrameBufferTextScreenDevice(fb);
            Console.SetOutputDevice(FrameBufferTextScreen);
        }

        public static IFile GetDevice(string devName)
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
