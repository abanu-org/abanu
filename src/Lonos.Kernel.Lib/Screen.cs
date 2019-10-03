// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using Mosa.Runtime;
using Mosa.Runtime.x86;

namespace Lonos.Kernel.Core
{
    /// <summary>
    /// Screen
    /// </summary>
    public static class Screen
    {

        public static void EarlyInitialization()
        {
            Rows = 25;
            Columns = 80;
            Clear();
            Goto(0, 0);
        }

        public static void ApplyMode(uint mode)
        {
            // https://de.wikibooks.org/wiki/Interrupts_80x86/_INT_10#Funktion_00h:_Setze_Bildschirmmodus_(EGA/VGA)
            KernelMessage.WriteLine("Screen VBE Mode: {0}", mode);
            switch (mode)
            {
                case 1:
                    Rows = 25;
                    Columns = 40;
                    break;
                case 3:
                    Rows = 25;
                    Columns = 80;
                    break;
            }
        }

        private static StringBuffer tmpLine;
        public static uint ScreenMemoryAddress = 0x0B8000;
        public static uint ScreenMemorySize = 25 * 80 * 2;

        public static uint _column = 0;
        public static uint _row = 0;
        private static byte color = 23;

        /// <summary>
        /// The columns
        /// </summary>
        public static uint Columns = 40;

        /// <summary>
        /// The rows
        /// </summary>
        public static uint Rows = 25;

        /// <summary>
        /// Gets or sets the column.
        /// </summary>
        /// <value>
        /// The column.
        /// </value>
        public static uint Column
        {
            get { return _column; }
            set { _column = value; }
        }

        /// <summary>
        /// Gets or sets the row.
        /// </summary>
        /// <value>
        /// The row.
        /// </value>
        public static uint Row
        {
            get { return _row; }
            set { _row = value; }
        }

        public static byte Color
        {
            get
            {
                return (byte)(color & 0x0F);
            }

            set
            {
                color &= 0xF0;
                color |= (byte)(value & 0x0F);
            }
        }

        public static byte BackgroundColor
        {
            get
            {
                return (byte)(color >> 4);
            }

            set
            {
                color &= 0x0F;
                color |= (byte)((value & 0x0F) << 4);
            }
        }

        /// <summary>
        /// Next Column
        /// </summary>
        private static void Next()
        {
            Column++;

            if (Column >= Columns)
            {
                NextLine();
            }
        }

        /// <summary>
        /// Skips the specified skip.
        /// </summary>
        /// <param name="skip">The skip.</param>
        private static void Skip(uint skip)
        {
            for (uint i = 0; i < skip; i++)
                Next();
        }

        /// <summary>
        /// Writes the character.
        /// </summary>
        public static void RawWrite(uint row, uint column, char chr, byte color)
        {
            Assert.True(row < Rows);
            Assert.True(column < Columns);
            Pointer address = new Pointer(ScreenMemoryAddress + (((row * Columns) + column) * 2));

            Intrinsic.Store8(address, (byte)chr);
            Intrinsic.Store8(address, 1, color);
        }

        /// <summary>
        /// Writes the character.
        /// </summary>
        /// <param name="chr">The character.</param>
        public static void Write(char chr)
        {
            if (chr == 10)
            {
                NextLine();
                return;
            }

            Pointer address = new Pointer(ScreenMemoryAddress + (((Row * Columns) + Column) * 2));

            Intrinsic.Store8(address, (byte)chr);
            Intrinsic.Store8(address, 1, color);

            Next();
            UpdateCursor();
        }

        /// <summary>
        /// Writes the string to the screen.
        /// </summary>
        /// <param name="value">The string value to write to the screen.</param>
        public static void Write(string value)
        {
            for (int index = 0; index < value.Length; index++)
            {
                char chr = value[index];
                if (chr == 10)
                {
                    NextLine();
                }
                else
                {
                    Write(chr);
                }
            }
        }

        /// <summary>
        /// Writes the string to the screen.
        /// </summary>
        /// <param name="value">The string value to write to the screen.</param>
        public static void Write(StringBuffer value)
        {
            for (int index = 0; index < value.Length; index++)
            {
                char chr = value[index];
                Write(chr);
            }
        }

        //public static void Write(uint val, string format)
        //{
        //    Write(new StringBuffer(val, format));
        //}

        /// <summary>
        /// Goto the top.
        /// </summary>
        public static void GotoTop()
        {
            Column = 0;
            Row = 0;
            UpdateCursor();
        }

        /// <summary>
        /// Goto the next line.
        /// </summary>
        public static void NextLine()
        {
            Column = 0;
            if (_row >= Rows - 1)
            {
                // Copy All rows one line up
                // TODO: Normally, Reading from mapped ROM is much slower
                // than reading from normal RAM. Consider using Offscreen Buffer
                MemoryOperation.Copy(ScreenMemoryAddress + (Columns * 2), ScreenMemoryAddress, (Rows - 1) * Columns * 2);

                //Blank last line
                for (uint c = 0; c < Columns; c++)
                    RawWrite(_row, c, ' ', color);
            }
            else
            {
                Row++;
            }
            UpdateCursor();
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public static void Clear()
        {
            GotoTop();

            byte c = Color;
            Color = 0x0;

            for (int i = 0; i < Columns * Rows; i++)
                Write(' ');

            Color = c;
            GotoTop();
        }

        /// <summary>
        /// Goto the specified row and column.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="col">The col.</param>
        public static void Goto(uint row, uint col)
        {
            Row = row;
            Column = col;
            UpdateCursor();
        }

        /// <summary>
        /// Sets the cursor.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="col">The col.</param>
        public static void SetCursor(uint row, uint col)
        {
            uint location = (row * Columns) + col;

            Native.Out8(0x3D4, 0x0F);
            Native.Out8(0x3D5, (byte)(location & 0xFF));

            Native.Out8(0x3D4, 0x0E);
            Native.Out8(0x3D5, (byte)((location >> 8) & 0xFF));
        }

        public static void UpdateCursor()
        {
            SetCursor(Row, Column);
        }

        public static void ClearRow()
        {
            uint c = Column;
            uint r = Row;

            Column = 0;

            for (int i = 0; i < Columns; i++)
            {
                Write(' ');
            }

            Goto(r, c);
        }

        /// <summary>
        /// Writes the specified value.
        /// </summary>
        /// <param name="val">The val.</param>
        public static void Write(uint val)
        {
            Write(val, 10, -1);
        }

        /// <summary>
        /// Writes the specified value.
        /// </summary>
        /// <param name="val">The val.</param>
        /// <param name="digits">Number base. Use 10 for decimal, 16 for hex</param>
        public static void Write(uint val, byte digits)
        {
            Write(val, digits, -1);
        }

        /// <summary>
        /// Writes the specified value.
        /// </summary>
        /// <param name="val">The val.</param>
        /// <param name="digits">Number base. Use 10 for decimal, 16 for hex</param>
        /// <param name="size">Size of value. Used for Padding / filling with zeros</param>
        public static void Write(uint val, byte digits, int size)
        {
            tmpLine.Clear();
            tmpLine.Append(val, digits, size);
            Write(tmpLine);
        }

        public static void Write_(uint val, byte digits, int size)
        {
            uint count = 0;
            uint temp = val;

            do
            {
                temp /= digits;
                count++;
            }
            while (temp != 0);

            if (size != -1)
                count = (uint)size;

            uint x = Column;
            uint y = Row;

            for (uint i = 0; i < count; i++)
            {
                uint digit = val % digits;
                Column = x;
                Row = y;
                Skip(count - 1 - i);
                if (digit < 10)
                    Write((char)('0' + digit));
                else
                    Write((char)('A' + digit - 10));
                val /= digits;
            }

            Column = x;
            Row = y;
            Skip(count);
            UpdateCursor();
        }

        public static void SetChar(char chr, uint row, uint column)
        {
            SetCharInternal(chr, row, column, color);
        }

        public static void SetChar(char chr, uint row, uint column, ConsoleColor foregroundColor)
        {
            SetCharInternal(chr, row, column, (byte)(((byte)foregroundColor & 0x0F) | (BackgroundColor << 4)));
        }

        public static void SetChar(char chr, uint row, uint column, ConsoleColor foregroundColor, ConsoleColor backgroundColor)
        {
            SetCharInternal(chr, row, column, (byte)(((byte)foregroundColor & 0x0F) | ((byte)backgroundColor << 4)));
        }

        private static void SetCharInternal(char chr, uint row, uint column, byte color)
        {
            Pointer address = new Pointer(ScreenMemoryAddress + (((row * Columns) + column) * 2));

            Intrinsic.Store8(address, (byte)chr);
            Intrinsic.Store8(address, 1, color);
        }

    }
}
