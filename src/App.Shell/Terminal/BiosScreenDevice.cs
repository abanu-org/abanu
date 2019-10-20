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

// https://www.csie.ntu.edu.tw/~r92094/c++/VT100.html
// http://www.termsys.demon.co.uk/vtansi.htm
// http://man7.org/linux/man-pages/man4/console_codes.4.html

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

    }

    /// <summary>
    /// Stateless Terminal Driver
    /// </summary>
    public class TerminalDevice
    {

        private BiosScreenDevice Device;

        public int Rows;
        public int Columns;

        public unsafe void SetChar(int row, int column, TerminalChar terminalChar)
        {
            Data[(row * Columns) + column] = terminalChar;
        }

        private unsafe TerminalChar* Data;
        private unsafe TerminalChar* Data2;

        public unsafe void Initialize(BiosScreenDevice dev)
        {
            Device = dev;

            Rows = dev.Rows;
            Columns = dev.Columns;

            Data = (TerminalChar*)RuntimeMemory.AllocateCleared(sizeof(TerminalChar) * Rows * Columns);
            Data2 = (TerminalChar*)RuntimeMemory.AllocateCleared(sizeof(TerminalChar) * Rows * Columns);
        }

        public unsafe void Clear(TerminalChar fillChar)
        {
            Clear(0, Rows * Columns, fillChar);
        }

        public unsafe void Clear(int start, int length, TerminalChar fillChar)
        {
            var totalLength = Rows * Columns;
            if (start + length > totalLength)
                length = totalLength - start;

            var c = &Data[start];
            var pos = 0;
            while (pos < length)
            {
                *c = fillChar;

                c++;
                pos++;
            }
        }

        public unsafe void Update()
        {
            Update(0, Rows * Columns);
        }

        public unsafe void Update(int start, int length)
        {
            var totalLength = Rows * Columns;
            if (start + length > totalLength)
                length = totalLength - start;

            var c = &Data[start];
            var c2 = &Data2[start];

            var pos = 0;
            var offset = start;
            while (pos < length)
            {
                if (!TerminalChar.Equals(c, c2))
                {
                    *c2 = *c;

                    Device.SetChar(start + pos, *c);
                }

                c++;
                c2++;
                pos++;
            }
        }

        public unsafe void ShiftUp()
        {
            // Copy All rows one line up
            // TODO: Normally, Reading from mapped ROM is much slower
            // than reading from normal RAM. Consider using Offscreen Buffer
            var sizePerChar = (uint)sizeof(TerminalChar);
            MemoryOperation.Copy4((uint)Data + ((uint)Columns * sizePerChar), (uint)Data, ((uint)Rows - 1) * (uint)Columns * sizePerChar);
        }

        public unsafe void ClearRow(int row, TerminalChar fillChar)
        {
            Clear(row * Columns, Columns, fillChar);
        }

        /// <summary>
        /// Sets the cursor.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="col">The col.</param>
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
