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

namespace Lonos.Kernel
{

    public class SysConsole
    {

        private IBuffer file;

        public void Init()
        {
            file = new ConsoleServer();
        }

        private void SendCommand()
        {

        }

        private void SendCommand(string command)
        {
            SendByte(SysConsoleConstants.ESC);
            SendByte('[');
            SendBytes(command);
        }

        private void SendByte(byte data)
        {

        }

        private void SendByte(char data)
        {
            file.Write(data);
        }

        private void SendBytes(string data)
        {
            file.Write(data);
        }

        public void Clear()
        {
            Write("\x001B[2J");
        }

        public void Reset()
        {
            Write("\x001B[c");
        }

        public void Write(string msg)
        {
            SendBytes(msg);
        }

        public void Write(uint value)
        {
            var str = value.ToString();
            Write(str);
            RuntimeMemory.FreeObject(str);
        }

        public void Write(int value)
        {
            var str = value.ToString();
            Write(str);
            RuntimeMemory.FreeObject(str);
        }

        public void WriteLine(string msg)
        {
            SendBytes(msg);
        }

        public void SetForegroundColor(byte color)
        {
            Write("\x001B[3");
            Write(color);
            Write("m");
        }

        public void SetBackgroundColor(byte color)
        {
            Write("\x001B[4");
            Write(color);
            Write("m");
        }

        public void SetCursor(int row, int column)
        {
            Write("\x001B[");
            Write(row);
            Write(";");
            Write(column);
            Write("H");
        }

        public void ApplyDefaultColor()
        {
            Write("\x001B[8]");
        }

        public void SetColor(byte color)
        {
        }

    }

    [Flags]
    public enum TerminalCharAttributes
    {
        None = 0,
        Bold = 1,
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TerminalChar
    {
        public char Char;
        public byte ForegroundColor;
        public byte BackgroundColor;
        public TerminalCharAttributes Attributes;
    }

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
        public const uint Rows = 40;

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

    public class ConsoleServer : IBuffer
    {

        internal static TerminalDevice Dev;

        public ConsoleServer()
        {
            Sequence = new List<char>();
            Dev = new TerminalDevice();
            Dev.Initialize();
        }

        public unsafe SSize Read(byte* buf, USize count)
        {
            return 0;
        }

        public unsafe SSize Write(byte* buf, USize count)
        {
            for (var i = 0; i < count; i++)
            {
                ProcessChar((char)buf[i]);
            }
            return (SSize)count;
        }

        private void ProcessChar(char b)
        {
            if (b == SysConsoleConstants.ESC)
            {
                SequenceBegin();
                return;
            }

            if (InSeqence)
            {
                ProcessSequenceChar(b);
                return;
            }

            WriteText(b);
        }

        private bool InSeqence;

        private void SequenceBegin()
        {
            InSeqence = true;
            Sequence.Clear();
        }

        private void SequenceEnd()
        {
            InSeqence = false;
            Sequence.Clear();
        }

        private List<char> Sequence;

        private void ProcessSequenceChar(char b)
        {
            Sequence.Add(b);
            ProcessSequence();
        }

        private void ProcessSequence()
        {
            if (TryModes())
                return;

            if (TrySetCursor())
                return;

            if (TryModes2())
                return;

            if (TryClear())
                return;

            if (TryReset())
                return;
        }

        private bool TryClear()
        {
            if (HasChars(SysConsoleConstants.ClarScreen))
            {
                Clear();
                SequenceEnd();
                return true;
            }
            return false;
        }

        private bool TryReset()
        {
            if (HasChars(SysConsoleConstants.Reset))
            {
                Reset();
                SequenceEnd();
                return true;
            }
            return false;
        }

        private bool TryModes()
        {
            if (Sequence.Count > 1 && char.IsDigit(Sequence[1]) && Sequence[Sequence.Count - 1] == 'm')
            {
                if (Sequence.Count == 2)
                {
                    // [m
                    AllModesOff();
                    SequenceEnd();
                    return true;
                }

                for (var i = 1; i < Sequence.Count - 1; i++)
                {

                    if (Sequence[i] == ';')
                        continue;

                    if (!char.IsDigit(Sequence[i + 1]))
                    {
                        // single digit
                        switch (Sequence[i])
                        {
                            case '0':
                                // [0m
                                AllModesOff();
                                break;
                        }
                    }
                    else
                    {
                        // set color mode
                        var colorChar = Sequence[i + 1];
                        byte color = colorChar.GetNumber();
                        if (Sequence[i] == '3')
                            SetForeColor(color);
                        else if (Sequence[i] == '4')
                            SetBackColor(color);
                    }
                }
                SequenceEnd();
                return true;
            }
            return false;
        }

        private bool TryModes2()
        {
            if (Sequence.Count > 1 && char.IsDigit(Sequence[1]) && Sequence[Sequence.Count - 1] == ']')
            {
                for (var i = 1; i < Sequence.Count - 1; i++)
                {

                    SysCalls.WriteDebugChar(Sequence[i]);

                    if (Sequence[i] == ';')
                        continue;

                    if (!char.IsDigit(Sequence[i + 1]))
                    {
                        // single digit
                        switch (Sequence[i])
                        {
                            case '8':
                                // [8]
                                StoreDefaultColor();
                                break;
                        }
                    }
                    else
                    {
                    }
                }
                SequenceEnd();
                return true;
            }
            return false;
        }

        private bool TrySetCursor()
        {
            if (Sequence.Count > 1 && char.IsDigit(Sequence[1]) && Sequence[Sequence.Count - 1] == 'H')
            {
                // [H and [;H
                if (Sequence.Count == 2 || Sequence.Count == 3)
                {
                    SetCursor(0, 0);
                    SequenceEnd();
                    return true;
                }

                var row = 0;
                var col = 0;
                var nextParamIndex = 1;
                for (var i = 1; i < Sequence.Count - 1; i++)
                {

                    if (Sequence[i] == ';')
                    {
                        row = (int)Sequence.ParseUInt32(nextParamIndex, i - 1);
                        nextParamIndex = i + 1;
                        col = (int)Sequence.ParseUInt32(nextParamIndex, Sequence.Count - nextParamIndex - 1);
                        SetCursor(row, col);
                        SequenceEnd();
                        return true;
                    }
                }
                SequenceEnd();
                return true;
            }
            return false;
        }

        private void SetCursor(int row, int column)
        {
            Row = row;
            Column = column;
        }

        private const byte SystemDefaultForeColor = 4;
        private const byte SystemDefaultBackColor = 5;
        private TerminalCharAttributes Attributes;

        private byte ForeColor;
        private byte BackColor;

        private void Reset()
        {
            DefaultForeColor = SystemDefaultForeColor;
            DefaultBackColor = SystemDefaultBackColor;
            AllModesOff();
            Clear();
        }

        private void SetForeColor(byte color)
        {
            ForeColor = color;
        }

        private void SetBackColor(byte color)
        {
            BackColor = color;
        }

        private void AllModesOff()
        {
            ForeColor = DefaultForeColor;
            BackColor = DefaultBackColor;
            Attributes = TerminalCharAttributes.None;
        }

        private void StoreDefaultColor()
        {
            SysCalls.WriteDebugChar('!');
            SysCalls.WriteDebugChar('D');
            DefaultForeColor = ForeColor;
            DefaultBackColor = BackColor;
        }

        private byte DefaultForeColor;
        private byte DefaultBackColor;

        private void Clear()
        {
            SysCalls.WriteDebugChar('!');
            SysCalls.WriteDebugChar('C');

            Dev.Clear(new TerminalChar
            {
                Char = ' ',
                ForegroundColor = DefaultForeColor,
                BackgroundColor = DefaultBackColor,
            });
        }

        private bool HasChars(string chars, int start = 0)
        {
            for (var i = 0; i < chars.Length; i++)
            {
                var idx = i + start;
                if (idx >= Sequence.Count)
                    return false;
                if (Sequence[idx] != chars[i])
                    return false;
            }
            return true;
        }

        private int Column;
        private int Row;

        private void NextLine()
        {
            Column = 0;
            if (Row >= TerminalDevice.Rows - 1)
            {
                Dev.ScrollUp();
                var blankChar = new TerminalChar
                {
                    Char = ' ',
                    BackgroundColor = 0,
                    ForegroundColor = 0,
                };

                //Blank last line
                for (int c = 0; c < TerminalDevice.Columns; c++)
                    Dev.SetChar(Row, c, blankChar);
            }
            else
            {
                Row++;
            }
            //UpdateCursor();
        }

        private void WriteText(char b)
        {
            if (b == 10)
            {
                NextLine();
                return;
            }

            Dev.SetChar(Row, Column, new TerminalChar
            {
                Char = b,
                ForegroundColor = ForeColor,
                BackgroundColor = BackColor,
                Attributes = Attributes,
            });
            Dev.Update();
            Next();
        }

        private void Next()
        {
            Column++;

            if (Column >= TerminalDevice.Columns)
                NextLine();
        }

    }

    public static class SysConsoleConstants
    {
        public const char ESC = (char)27;
        public static readonly string Reset = "c";
        public static readonly string ClarScreen = "[2J";
        public static readonly string ModesOff = "[0m";
        public static readonly string BoldOn = "[1m";
    }

}
