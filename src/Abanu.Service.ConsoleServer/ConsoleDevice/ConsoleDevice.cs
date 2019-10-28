// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

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

    /// <summary>
    /// Stateless Terminal Driver
    /// </summary>
    public class ConsoleDevice
    {

        private ITextConsoleDevice Device;

        public int Rows;
        public int Columns;

        public unsafe void SetChar(int row, int column, ConsoleChar terminalChar)
        {
            PrimaryBuffer.Chars[(row * Columns) + column] = terminalChar;
        }

        /// <summary>
        /// Data is written primarily to <see cref="PrimaryBuffer"/>
        /// </summary>
        private ConsoleBuffer PrimaryBuffer;

        /// <summary>
        /// A copy of written data to the underlining device is held in <see cref="CachedBuffer"/>
        /// to avoid unnecessary writes to the hardware.
        /// </summary>
        private ConsoleBuffer CachedBuffer;

        public unsafe void Initialize(ITextConsoleDevice dev)
        {
            Device = dev;

            Rows = dev.Rows;
            Columns = dev.Columns;

            PrimaryBuffer = new ConsoleBuffer(this);
            CachedBuffer = new ConsoleBuffer(this);
        }

        public unsafe void Clear(ConsoleChar fillChar)
        {
            PrimaryBuffer.Clear(fillChar);
        }

        public unsafe void Clear(int start, int length, ConsoleChar fillChar)
        {
            PrimaryBuffer.Clear(start, length, fillChar);
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

            var primary = &PrimaryBuffer.Chars[start];
            var cached = &CachedBuffer.Chars[start];

            var pos = 0;
            var offset = start;
            while (pos < length)
            {
                if (!ConsoleChar.Equals(primary, cached))
                {
                    *cached = *primary;

                    Device.SetChar(start + pos, *primary);
                }

                primary++;
                cached++;
                pos++;
            }

            if (PrimaryBuffer.CursorRow != CachedBuffer.CursorRow
                || PrimaryBuffer.CursorColumn != CachedBuffer.CursorColumn)
            {
                CachedBuffer.SetCursor(PrimaryBuffer.CursorRow, PrimaryBuffer.CursorColumn);
                Device.SetCursor(CachedBuffer.CursorRow, CachedBuffer.CursorColumn);
            }
        }

        public unsafe void ShiftUp()
        {
            PrimaryBuffer.ShiftUp();
        }

        public unsafe void ClearRow(int row, ConsoleChar fillChar)
        {
            PrimaryBuffer.ClearRow(row, fillChar);
        }

        public void SetCursor(int row, int column)
        {
            PrimaryBuffer.SetCursor(row, column);
        }

        private class ConsoleBuffer
        {
            public unsafe ConsoleChar* Chars;

            public int CursorRow = 0;
            public int CursorColumn = 0;

            private int Rows = 0;
            private int Columns = 0;

            public unsafe ConsoleBuffer(ConsoleDevice dev)
            {
                Rows = dev.Rows;
                Columns = dev.Columns;
                Chars = (ConsoleChar*)RuntimeMemory.AllocateCleared(sizeof(ConsoleChar) * Rows * Columns);
            }

            public unsafe void ShiftUp()
            {
                var sizePerChar = (uint)sizeof(ConsoleChar);
                MemoryOperation.Copy4((uint)Chars + ((uint)Columns * sizePerChar), (uint)Chars, ((uint)Rows - 1) * (uint)Columns * sizePerChar);
            }

            public void SetCursor(int row, int column)
            {
                CursorRow = row;
                CursorColumn = column;
            }

            public unsafe void Clear(ConsoleChar fillChar)
            {
                Clear(0, Rows * Columns, fillChar);
            }

            public unsafe void Clear(int start, int length, ConsoleChar fillChar)
            {
                var totalLength = Rows * Columns;
                if (start + length > totalLength)
                    length = totalLength - start;

                var c = &Chars[start];
                var pos = 0;
                while (pos < length)
                {
                    *c = fillChar;

                    c++;
                    pos++;
                }
            }

            public unsafe void ClearRow(int row, ConsoleChar fillChar)
            {
                Clear(row * Columns, Columns, fillChar);
            }

        }

    }

}
