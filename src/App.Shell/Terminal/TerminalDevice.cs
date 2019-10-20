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
            CurrentBuffer[(row * Columns) + column] = terminalChar;
        }

        /// <summary>
        /// Data is written primarily to <see cref="CurrentBuffer"/>
        /// </summary>
        private unsafe TerminalChar* CurrentBuffer;

        /// <summary>
        /// A copy of written data to the underlining device is held in <see cref="PrimaryBuffer"/>
        /// to avoid unnecessary writes to the hardware.
        /// </summary>
        private unsafe TerminalChar* PrimaryBuffer;

        public unsafe void Initialize(BiosScreenDevice dev)
        {
            Device = dev;

            Rows = dev.Rows;
            Columns = dev.Columns;

            CurrentBuffer = (TerminalChar*)RuntimeMemory.AllocateCleared(sizeof(TerminalChar) * Rows * Columns);
            PrimaryBuffer = (TerminalChar*)RuntimeMemory.AllocateCleared(sizeof(TerminalChar) * Rows * Columns);
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

            var c = &CurrentBuffer[start];
            var pos = 0;
            while (pos < length)
            {
                *c = fillChar;

                c++;
                pos++;
            }
        }

        public unsafe void Flush()
        {
            Flush(0, Rows * Columns);
        }

        public unsafe void Flush(int start, int length)
        {
            var totalLength = Rows * Columns;
            if (start + length > totalLength)
                length = totalLength - start;

            var primary = &CurrentBuffer[start];
            var cached = &PrimaryBuffer[start];

            var pos = 0;
            var offset = start;
            while (pos < length)
            {
                if (!TerminalChar.Equals(primary, cached))
                {
                    *cached = *primary;

                    Device.SetChar(start + pos, *primary);
                }

                primary++;
                cached++;
                pos++;
            }

            Device.SetCursor(CursorRow, CursorColumn);
        }

        public unsafe void ShiftUp()
        {
            // Copy All rows one line up
            // TODO: Normally, Reading from mapped ROM is much slower
            // than reading from normal RAM. Consider using Offscreen Buffer
            var sizePerChar = (uint)sizeof(TerminalChar);
            MemoryOperation.Copy4((uint)CurrentBuffer + ((uint)Columns * sizePerChar), (uint)CurrentBuffer, ((uint)Rows - 1) * (uint)Columns * sizePerChar);
        }

        public unsafe void ClearRow(int row, TerminalChar fillChar)
        {
            Clear(row * Columns, Columns, fillChar);
        }

        private int CursorRow = 0;
        private int CursorColumn = 0;

        public void SetCursor(int row, int column)
        {
            CursorRow = row;
            CursorColumn = column;
        }

    }

}
