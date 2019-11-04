// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Abanu;
using Abanu.Kernel.Core;
using Abanu.Runtime;
using Mosa.Runtime.x86;

namespace Abanu.Kernel
{

    public class BiosTextConsoleDevice : ITextConsoleDevice
    {
        private int _Rows;
        public int Rows => _Rows;
        private int _Columns;
        public int Columns => _Columns;

        private Addr BaseAddr;

        public void Initialize()
        {
            _Rows = 25;
            _Columns = 80;
            BaseAddr = SysCalls.GetPhysicalMemory(0x0B8000, (uint)(_Rows * _Columns * 2));
        }

        public void SetChar(int row, int column, ConsoleChar c)
        {
            SetChar((row * _Columns) + column, c);
        }

        public unsafe void SetChar(int offset, ConsoleChar c)
        {
            var s = (byte*)(BaseAddr + (offset * 2));
            *s = (byte)c.Char;
            s++;
            *s = (byte)((c.ForegroundColor & 0x0F) | (c.BackgroundColor << 4));
        }

        public void SetCursor(int row, int col)
        {
            int location = (row * _Columns) + col;

            Native.Out8(0x3D4, 0x0F);
            Native.Out8(0x3D5, (byte)(location & 0xFF));

            Native.Out8(0x3D4, 0x0E);
            Native.Out8(0x3D5, (byte)((location >> 8) & 0xFF));
        }

    }

}
