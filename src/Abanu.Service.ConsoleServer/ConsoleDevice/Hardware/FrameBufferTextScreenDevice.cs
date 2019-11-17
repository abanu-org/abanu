// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abanu.Kernel.Core;
using Abanu.Kernel.Core.ConsoleFonts;
using Abanu.Runtime;

namespace Abanu.Kernel
{
    public class FrameBufferTextScreenDevice : ITextConsoleDevice
    {

        private IGraphicsAdapter dev;

        public FrameBufferTextScreenDevice(IGraphicsAdapter dev)
        {
            this.dev = dev;
            Columns = dev.Target.Width / CharWidth;
            Rows = dev.Target.Height / CharHeight;
            SetupColors();
        }

        public int Columns { get; private set; }

        public int Rows { get; private set; }

        private int CharHeight = 14;
        private int CharWidth = 8;

        // important Note: Do not cause Console Output while drawing
        // otherwise, a stack overflow will occur!
        internal unsafe void DrawChar(int row, int column, int charIdx, byte foregroundColor, byte backgroundColor)
        {
            if (column >= Columns || row >= Rows)
                return;

            //var fontSecAddr = ApplicationRuntime.ElfSections["consolefont.regular"];
            RuntimeElfSection fontSecAddr = null;
            return;

            var fontHeader = (PSF1Header*)fontSecAddr.Data.Start;

            //KernelMemory.DumpToConsole(fontSecAddr, 20);

            var rows = fontHeader->Charsize;
            var bytesPerRow = 1; //14 bits --> 2 bytes + 2fill bits
            int columns = 8;

            var charSize = bytesPerRow * rows;

            var charMem = (byte*)(fontSecAddr.Data.Start + sizeof(PSF1Header));
            //KernelMemory.DumpToConsole((uint)charMem, 20);

            var foreColor = GetNativeColor(foregroundColor);
            var backColor = GetNativeColor(backgroundColor);

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    var pixelX = (column * columns) + x;
                    var pixelY = (row * rows) + y;
                    var bt = BitHelper.IsBitSet(charMem[(charSize * charIdx) + (y * bytesPerRow) + (x / 8)], (byte)(7 - (x % 8)));
                    if (bt)
                    {
                        dev.SetPixel(pixelX, pixelY, foreColor);
                    }
                    else
                    {
                        dev.SetPixel(pixelX, pixelY, backColor);
                    }
                }
            }

        }

        private static uint[] Colors;

        private static void SetupColors()
        {
            Colors = new uint[]
            {
                0x00000000, // black
                0x000000AA, // blue
                0x0000AA00, // green
                0x0000AAAA, // cyan
                0x00AA0000, // red
                0x00AA00AA, // magenta
                0x00AA5500, // Brown / Yellow
                0x00AAAAAA, // light gray

                0x00555555, // Dark gray
                0x005555FF, // light blue
                0x0055FF55, // light green
                0x0055FFFF, // light cyan
                0x00FF5555, // light red
                0x00FF55FF, // light magenta
                0x00FFFF55, // Yellow
                0x00FFFFFF, // white
            };
        }

        private static uint GetNativeColor(byte consoleColor)
        {
            return Colors[consoleColor];
        }

        //private static int GetColor(byte consoleColor)
        //{
        //    // ARGB
        //    uint a = 0;
        //    uint r = 0xFF;
        //    uint g = 0;
        //    uint b = 0;
        //    uint value = a;
        //    value = (value << 8) | r;
        //    value = (value << 8) | g;
        //    value = (value << 8) | b;
        //    return (int)value;
        //}

        public void SetChar(int row, int column, ConsoleChar c)
        {
            DrawChar(row, column, (byte)c.Char, c.ForegroundColor, c.BackgroundColor);
        }

        public void SetChar(int offset, ConsoleChar c)
        {
            var row = offset / Columns;
            var col = offset % Columns;
            DrawChar(row, col, (byte)c.Char, c.ForegroundColor, c.BackgroundColor);
        }

        public void SetCursor(int row, int col)
        {
        }
    }
}
