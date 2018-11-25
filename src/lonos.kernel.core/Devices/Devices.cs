using System;
using Mosa.Runtime;
using Mosa.Kernel.x86;


namespace lonos.kernel.core
{

    public static class Devices
    {

        public static IFile Serial1;
        public static IFile Screen;
        public static IFile Console;
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
            Screen = new ScreenDevice();

            Console = new ConsoleDevice();
        }

        /// <summary>
        /// Video Stage
        /// </summary>
        public unsafe static void InitFrameBuffer()
        {
            fb = new FrameBuffer(Multiboot.multiBootInfo->FbAddr, Multiboot.multiBootInfo->FbWidth, Multiboot.multiBootInfo->FbHeight, Multiboot.multiBootInfo->FbPitch, 8);
            fb.Init();
            fb.FillRectangle(uint.MaxValue, 0, 0, 400, 400);
            fb.SetPixel(uint.MaxValue/2, 0, 1);
            fb.SetPixel(uint.MaxValue / 4, 0, 2);
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
