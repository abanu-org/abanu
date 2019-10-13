// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Runtime.InteropServices;
using Mosa.Runtime.x64;

namespace Lonos.Kernel.Core.Elf
{

    [StructLayout(LayoutKind.Explicit)]
    public struct ElfSymbol
    {
        [FieldOffset(0)]
        public uint Name;
        [FieldOffset(4)]
        public uint Value;
        [FieldOffset(8)]
        public uint Size;
        [FieldOffset(12)]
        public byte Info;
        [FieldOffset(13)]
        public byte Other;
        [FieldOffset(14)]
        public ushort ShNdx;
    }

}
