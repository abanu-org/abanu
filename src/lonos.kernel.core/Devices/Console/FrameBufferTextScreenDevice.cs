
using System;

namespace lonos.kernel.core
{

    public class FrameBufferTextScreenDevice : IFile
    {

        private FrameBuffer dev;

        public FrameBufferTextScreenDevice(FrameBuffer dev)
        {
            this.dev = dev;
            Columns = 640 / 8;
            Rows = 480 / 14;
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
            _row++;
            _col = 0;
        }

        // important Note: Not not cause Console Output while drawing
        // otherwise, a stack overflow will occur!
        internal unsafe void DrawChar(FrameBuffer fb, uint screenX, uint screenY, uint charIdx)
        {
            var fontSec = KernelElf.Main.GetSectionHeader("consolefont.regular");
            var fontSecAddr = KernelElf.Main.GetSectionPhysAddr(fontSec);

            var fontHeader = (PSF1Header*)fontSecAddr;

            //KernelMemory.DumpToConsole(fontSecAddr, 20);

            var rows = fontHeader->charsize;
            var bytesPerRow = 1; //14 bits --> 2 bytes + 2fill bits
            uint columns = 8;

            var charSize = bytesPerRow * rows;

            var charMem = (byte*)(fontSecAddr + sizeof(PSF1Header));
            //KernelMemory.DumpToConsole((uint)charMem, 20);

            for (uint y = 0; y < rows; y++)
            {
                for (uint x = 0; x < columns; x++)
                {
                    var bt = BitHelper.IsBitSet(charMem[charSize * charIdx + (y * bytesPerRow + (x / 8))], (byte)(7 - (x % 8)));
                    if (bt)
                    {
                        fb.SetPixel(int.MaxValue / 2, screenX * columns + x, screenY * rows + y);
                    }
                }
            }

        }

    }

}
