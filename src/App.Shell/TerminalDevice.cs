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

#pragma warning disable CA1822 // Mark members as static

// https://www.csie.ntu.edu.tw/~r92094/c++/VT100.html
// http://www.termsys.demon.co.uk/vtansi.htm
// http://man7.org/linux/man-pages/man4/console_codes.4.html

namespace Lonos.Kernel
{

    /// <summary>
    /// Stateless Terminal Driver
    /// </summary>
    public class TerminalDevice
    {

        public unsafe void SetChar(int row, int column, TerminalChar terminalChar)
        {
            Data[(row * Columns) + column] = terminalChar;
        }

        public const uint Columns = 80;
        public const uint Rows = 25;

        private Addr BaseAddr;
        private unsafe TerminalChar* Data;

        public unsafe void Initialize()
        {
            BaseAddr = SysCalls.GetPhysicalMemory(0x0B8000, Rows * Columns * 2);
            Data = (TerminalChar*)RuntimeMemory.AllocateCleared(sizeof(TerminalChar) * Rows * Columns);
        }

        public unsafe void Clear(TerminalChar fillChar)
        {
            Clear(0, Rows * Columns, fillChar);
        }

        public unsafe void Clear(uint start, uint length, TerminalChar fillChar)
        {
            var totalLength = Rows * Columns;
            if (start + length > totalLength)
                length = totalLength - start;

            var c = Data;
            var s = (byte*)BaseAddr + (start * 2);
            var pos = 0;
            var color = (byte)((fillChar.ForegroundColor & 0x0F) | (fillChar.BackgroundColor << 4));
            while (pos < length)
            {
                *c = fillChar;

                *s = (byte)' ';
                s++;

                *s = color;
                s++;

                c++;
                pos++;
            }
        }

        public unsafe void Update()
        {
            Update(0, Rows * Columns);
        }

        public unsafe void Update(uint start, uint length)
        {
            var totalLength = Rows * Columns;
            if (start + length > totalLength)
                length = totalLength - start;

            var c = Data;
            var s = (byte*)BaseAddr + (start * 2);
            var pos = 0;
            while (pos < length)
            {
                *s = (byte)c->Char;
                s++;

                *s = (byte)((c->ForegroundColor & 0x0F) | (c->BackgroundColor << 4));
                s++;

                c++;
                pos++;
            }
        }

        public unsafe void ScrollUp()
        {
            // Copy All rows one line up
            // TODO: Normally, Reading from mapped ROM is much slower
            // than reading from normal RAM. Consider using Offscreen Buffer
            var sizePerChar = (uint)sizeof(TerminalChar);
            MemoryOperation.Copy4((uint)Data + (Columns * sizePerChar), (uint)Data, (Rows - 1) * Columns * sizePerChar);
        }
    }

}
