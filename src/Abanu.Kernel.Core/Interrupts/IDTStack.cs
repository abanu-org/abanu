// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System.Runtime.InteropServices;

namespace Abanu.Kernel.Core.Interrupts
{

    /// <summary>
    /// Holds informations of the interrupted process and the raised interrupt
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct IDTStack
    {
        public const int Size = 52;

        public static class Offset
        {
            public const int EDI = 0x00;
            public const int ESI = 0x04;
            public const int EBP = 0x08;
            public const int ESP = 0x0C;
            public const int EBX = 0x10;
            public const int EDX = 0x14;
            public const int ECX = 0x18;
            public const int EAX = 0x1C;
            public const int Interrupt = 0x20;
            public const int ErrorCode = 0x24;
            public const int EIP = 0x28;
            public const int CS = 0x2C;
            public const int EFLAGS = 0x30;
            public const int TASK_ESP = 0x34;
            public const int TASK_SS = 0x38;
        }

        [FieldOffset(Offset.EDI)]
        public uint EDI;

        [FieldOffset(Offset.ESI)]
        public uint ESI;

        [FieldOffset(Offset.EBP)]
        public uint EBP;

        [FieldOffset(Offset.ESP)]
        public uint ESP;

        [FieldOffset(Offset.EBX)]
        public uint EBX;

        [FieldOffset(Offset.EDX)]
        public uint EDX;

        [FieldOffset(Offset.ECX)]
        public uint ECX;

        [FieldOffset(Offset.EAX)]
        public uint EAX;

        [FieldOffset(Offset.Interrupt)]
        public uint Interrupt;

        [FieldOffset(Offset.ErrorCode)]
        public uint ErrorCode;

        [FieldOffset(Offset.EIP)]
        public uint EIP;

        [FieldOffset(Offset.CS)]
        public uint CS;

        [FieldOffset(Offset.EFLAGS)]
        public X86_EFlags EFLAGS;
    }
}
