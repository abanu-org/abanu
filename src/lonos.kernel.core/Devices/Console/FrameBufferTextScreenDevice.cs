// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using Lonos.Kernel.Core.ConsoleFonts;
using Lonos.Kernel.Core.Devices;
using Lonos.Kernel.Core.Elf;

namespace Lonos.Kernel.Core
{

    public class FrameBufferTextScreenDevice : IFile
    {

        private FrameBuffer dev;

        public FrameBufferTextScreenDevice(FrameBuffer dev)
        {
            this.dev = dev;
            Columns = dev.Width / CharWidth;
            Rows = dev.Height / CharHeight;
        }

        public unsafe SSize Write(byte* buf, USize count)
        {
            for (var i = 0; i < count; i++)
            {
                Write((char)buf[i]);
            }

            return (uint)count;
        }

        private uint _row;
        private uint _col;

        public uint Columns { get; private set; }

        public uint Rows { get; private set; }

        private void Write(char c)
        {
            if (c == '\n')
            {
                NextLine();
                return;
            }
            DrawChar(dev, _col, _row, (byte)c);
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

        private uint CharHeight = 14;
        private uint CharWidth = 8;

        // important Note: Not not cause Console Output while drawing
        // otherwise, a stack overflow will occur!
        internal unsafe void DrawChar(FrameBuffer fb, uint screenX, uint screenY, uint charIdx)
        {
            if (screenX >= Columns || screenY >= Rows)
                return;

            var fontSec = KernelElf.Main.GetSectionHeader("consolefont.regular");
            var fontSecAddr = KernelElf.Main.GetSectionPhysAddr(fontSec);

            var fontHeader = (PSF1Header*)fontSecAddr;

            //KernelMemory.DumpToConsole(fontSecAddr, 20);

            var rows = fontHeader->Charsize;
            var bytesPerRow = 1; //14 bits --> 2 bytes + 2fill bits
            uint columns = 8;

            var charSize = bytesPerRow * rows;

            var charMem = (byte*)(fontSecAddr + sizeof(PSF1Header));
            //KernelMemory.DumpToConsole((uint)charMem, 20);

            for (uint y = 0; y < rows; y++)
            {
                for (uint x = 0; x < columns; x++)
                {
                    var bt = BitHelper.IsBitSet(charMem[(charSize * charIdx) + (y * bytesPerRow) + (x / 8)], (byte)(7 - (x % 8)));
                    if (bt)
                    {
                        fb.SetPixel(int.MaxValue / 2, (screenX * columns) + x, (screenY * rows) + y);
                    }
                }
            }

        }

    }

}
