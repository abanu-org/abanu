// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Lonos;
using Lonos.Kernel.Core;
using Lonos.Runtime;
using Mosa.Runtime.x86;

#pragma warning disable CA1822 // Mark members as static

namespace Lonos.Kernel
{

    public class BiosScreenDevice
    {
        public int Rows;
        public int Columns;

        private Addr BaseAddr;

        public void Initialize()
        {
            Rows = 25;
            Columns = 80;
            BaseAddr = SysCalls.GetPhysicalMemory(0x0B8000, (uint)(Rows * Columns * 2));
        }

        public void SetChar(int row, int column, TerminalChar c)
        {
        }

        public unsafe void SetChar(int offset, TerminalChar c)
        {
            var s = (byte*)(BaseAddr + (offset * 2));
            *s = (byte)c.Char;
            s++;
            *s = (byte)((c.ForegroundColor & 0x0F) | (c.BackgroundColor << 4));
        }

        public void SetCursor(int row, int col)
        {
            int location = (row * Columns) + col;

            Native.Out8(0x3D4, 0x0F);
            Native.Out8(0x3D5, (byte)(location & 0xFF));

            Native.Out8(0x3D4, 0x0E);
            Native.Out8(0x3D5, (byte)((location >> 8) & 0xFF));
        }

    }

}
