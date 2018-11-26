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
            test_DrawChar(fb, 0, 0, 0);
            test_DrawChar(fb, 1, 1, 1);
            test_DrawChar(fb, 2, 2, 2);
        }

        private unsafe static void test_DrawChar(FrameBuffer fb, uint screenX, uint screenY, uint charIdx)
        {
            var fontSec = KernelElf.Main.GetSectionHeader("consolefont.regular");
            var fontSecAddr = KernelElf.Main.GetSectionPhysAddr(fontSec);

            var fontHeader = (PSF1Header*)fontSecAddr;

            KernelMemory.DumpToConsole(fontSecAddr, 20);

            var rows = fontHeader->charsize;
            var bytesPerRow = 1; //14 bits --> 2 bytes + 2fill bits
            uint columns = 8;

            var charSize = bytesPerRow * rows;

            var charMem = (byte*)(fontSecAddr + sizeof(PSF1Header));
            KernelMemory.DumpToConsole((uint)charMem, 20);

            for (uint y = 0; y < rows; y++)
            {
                for (uint x = 0; x < columns; x++)
                {
                    var bt = BitHelper.IsBitSet(charMem[charSize * charIdx + (y * bytesPerRow + (x / 8))], (byte)(7-(x % 8)));
                    if (bt)
                    {
                        fb.SetPixel(int.MaxValue / 2, screenX * columns + x, screenY * rows + y);
                    }
                }
            }

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
