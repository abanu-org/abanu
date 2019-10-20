// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System.Runtime.InteropServices;

#pragma warning disable CA1822 // Mark members as static

namespace Lonos.Kernel
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TerminalChar
    {
        public char Char;
        public byte ForegroundColor;
        public byte BackgroundColor;
        public TerminalCharAttributes Attributes;

        public static unsafe bool Equals(TerminalChar c1, TerminalChar c2)
        {
            return Equals(&c1, &c2);
        }

        public static unsafe bool Equals(TerminalChar* c1, TerminalChar* c2)
        {
            return c1->Char == c2->Char
              && c1->ForegroundColor == c2->ForegroundColor
              && c1->BackgroundColor == c2->BackgroundColor
              && c1->Attributes == c2->Attributes;
        }
    }

}
