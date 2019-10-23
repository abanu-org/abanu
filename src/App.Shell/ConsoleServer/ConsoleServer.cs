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

// https://www.csie.ntu.edu.tw/~r92094/c++/VT100.html
// http://www.termsys.demon.co.uk/vtansi.htm
// http://man7.org/linux/man-pages/man4/console_codes.4.html

namespace Lonos.Kernel
{

    public class ConsoleServer : IBuffer
    {

        internal static ConsoleDevice Dev;

        public ConsoleServer()
        {
            Sequence = new List<char>();

            ITextConsoleDevice txtDev;

            var fb = FrameBuffer.Create();
            if (fb != null)
            {
                var fbTxt = new FrameBufferTextScreenDevice(fb);
                txtDev = fbTxt;
            }
            else
            {
                var biosScreen = new BiosTextConsoleDevice();
                biosScreen.Initialize();
                txtDev = biosScreen;
            }

            Dev = new ConsoleDevice();
            Dev.Initialize(txtDev);
        }

        public unsafe SSize Read(byte* buf, USize count)
        {
            return 0;
        }

        public unsafe SSize Write(byte* buf, USize count)
        {
            for (var i = 0; i < count; i++)
                ProcessChar((char)buf[i]);

            Flush();
            return (SSize)count;
        }

        private void ProcessChar(char b)
        {
            if (b == ConsoleServerConstants.ESC)
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
            if (HasChars(ConsoleServerConstants.ClarScreen))
            {
                Clear();
                SequenceEnd();
                return true;
            }
            return false;
        }

        private bool TryReset()
        {
            if (HasChars(ConsoleServerConstants.Reset))
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
        private ConsoleCharAttributes Attributes;

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
            Attributes = ConsoleCharAttributes.None;
        }

        private void StoreDefaultColor()
        {
            DefaultForeColor = ForeColor;
            DefaultBackColor = BackColor;
        }

        private byte DefaultForeColor;
        private byte DefaultBackColor;

        private void Clear()
        {
            Dev.Clear(new ConsoleChar
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
            if (Row >= Dev.Rows - 1)
            {
                Dev.ShiftUp();
                var blankChar = new ConsoleChar
                {
                    Char = ' ',
                    ForegroundColor = DefaultForeColor,
                    BackgroundColor = DefaultBackColor,
                };

                Dev.ClearRow(Row, blankChar);
            }
            else
            {
                Row++;
            }
            UpdateCursor();
        }

        private void UpdateCursor()
        {
            Dev.SetCursor(Row, Column);
        }

        private void WriteText(char b)
        {
            if (b == 10)
            {
                NextLine();
                return;
            }

            Dev.SetChar(Row, Column, new ConsoleChar
            {
                Char = b,
                ForegroundColor = ForeColor,
                BackgroundColor = BackColor,
                Attributes = Attributes,
            });
            Next();
            UpdateCursor();
        }

        private void Flush()
        {
            Dev.Flush();
        }

        private void Next()
        {
            Column++;

            if (Column >= Dev.Columns)
                NextLine();
        }

    }

}
