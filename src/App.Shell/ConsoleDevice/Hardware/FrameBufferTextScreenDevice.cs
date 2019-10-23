// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lonos.Kernel.Core;
using Lonos.Kernel.Core.ConsoleFonts;
using Lonos.Runtime;

namespace Lonos.Kernel
{
    public class FrameBufferTextScreenDevice : IBuffer, ITextConsoleDevice
    {

        private FrameBuffer dev;

        public FrameBufferTextScreenDevice(FrameBuffer dev)
        {
            this.dev = dev;
            Columns = dev.Width / CharWidth;
            Rows = dev.Height / CharHeight;
        }

        //public void Init()
        //{
        //}

        public unsafe SSize Write(byte* buf, USize count)
        {
            for (var i = 0; i < count; i++)
            {
                Write((char)buf[i]);
            }

            return (uint)count;
        }

        private int _row;
        private int _col;

        public int Columns { get; private set; }

        public int Rows { get; private set; }

        private void Write(char c)
        {
            if (c == '\n')
            {
                NextLine();
                return;
            }
            DrawChar(_col, _row, (byte)c);
            Next();
        }

        private void Next()
        {
            _col++;

            if (_col >= Columns)
            {
                NextLine();
            }
        }

        private void NextLine()
        {
            _col = 0;
            if (_row >= Rows - 1)
                Scroll();
            else
                _row++;
        }

        private void Scroll()
        {
            // TODO: Scroll
            _row = 0;
            dev.FillRectangle(0, 0, 0, dev.Width, dev.Height);
        }

        private int CharHeight = 14;
        private int CharWidth = 8;

        // important Note: Do not cause Console Output while drawing
        // otherwise, a stack overflow will occur!
        internal unsafe void DrawChar(int row, int column, int charIdx)
        {
            if (column >= Columns || row >= Rows)
                return;

            var fontSec = ApplicationRuntime.ElfSections.GetSectionHeader("consolefont.regular");
            var fontSecAddr = ApplicationRuntime.ElfSections.GetSectionPhysAddr(fontSec);

            var fontHeader = (PSF1Header*)fontSecAddr;

            //KernelMemory.DumpToConsole(fontSecAddr, 20);

            var rows = fontHeader->Charsize;
            var bytesPerRow = 1; //14 bits --> 2 bytes + 2fill bits
            int columns = 8;

            var charSize = bytesPerRow * rows;

            var charMem = (byte*)(fontSecAddr + sizeof(PSF1Header));
            //KernelMemory.DumpToConsole((uint)charMem, 20);

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    var pixelX = (column * columns) + x;
                    var pixelY = (row * rows) + y;
                    var bt = BitHelper.IsBitSet(charMem[(charSize * charIdx) + (y * bytesPerRow) + (x / 8)], (byte)(7 - (x % 8)));
                    if (bt)
                    {
                        dev.SetPixel(int.MaxValue / 2, pixelX, pixelY);
                    }
                    else
                    {
                        dev.SetPixel(0, pixelX, pixelY);
                    }
                }
            }

        }

        public unsafe SSize Read(byte* buf, USize count)
        {
            return 0;
        }

        public void SetChar(int row, int column, ConsoleChar c)
        {
            DrawChar(row, column, (byte)c.Char);
        }

        public void SetChar(int offset, ConsoleChar c)
        {
            var row = offset / Columns;
            var col = offset % Columns;
            DrawChar(row, col, (byte)c.Char);
        }

        public void SetCursor(int row, int col)
        {
        }
    }
}
