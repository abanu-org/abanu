// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using Abanu.Kernel.Core.ConsoleFonts;
using Abanu.Kernel.Core.Devices;
using Abanu.Kernel.Core.Elf;

namespace Abanu.Kernel.Core
{

    /// <summary>
    /// Basic framebuffer text output.
    /// </summary>
    public class FrameBufferTextScreenDevice : IBuffer
    {

        private IFrameBuffer dev;

        public FrameBufferTextScreenDevice(IFrameBuffer dev)
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
            dev.FillRectangle(0, 0, dev.Width, dev.Height, 0);
        }

        private int CharHeight = 14;
        private int CharWidth = 8;

        // important Note: Do not cause Console Output while drawing
        // otherwise, a stack overflow will occur!
        private unsafe void DrawChar(int screenX, int screenY, int charIdx)
        {
            if (screenX >= Columns || screenY >= Rows)
                return;

            // TODO: Improve
            var fontSec = KernelElf.Main.GetSectionHeader("consolefont.regular");
            var fontSecAddr = KernelElf.Main.GetSectionPhysAddr(fontSec);

            var fontHeader = (PSF1Header*)fontSecAddr;

            var rows = (int)fontHeader->Charsize;
            var bytesPerRow = 1; //14 bits --> 2 bytes + 2fill bits
            int columns = 8;

            var charSize = bytesPerRow * rows;

            var charMem = (byte*)(fontSecAddr + sizeof(PSF1Header));

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    var bt = BitHelper.IsBitSet(charMem[(charSize * charIdx) + (y * bytesPerRow) + (x / 8)], (byte)(7 - (x % 8)));
                    if (bt)
                    {
                        dev.SetPixel((screenX * columns) + x, (screenY * rows) + y, int.MaxValue / 2);
                    }
                }
            }

        }

        /// <summary>
        /// Reading text from Framebuffer is not supported
        /// </summary>
        public unsafe SSize Read(byte* buf, USize count)
        {
            throw new NotSupportedException();
        }

    }

}
