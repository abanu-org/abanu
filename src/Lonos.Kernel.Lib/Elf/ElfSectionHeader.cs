// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Runtime.InteropServices;
using Mosa.Runtime.x64;

namespace Lonos.Kernel.Core.Elf
{

    [StructLayout(LayoutKind.Sequential)]
    public struct ElfSectionHeader
    {
        public uint Name;
        public uint Type;
        public uint Flags;
        public uint Addr;
        public uint Offset;
        public uint Size;
        public uint Link;
        public uint Info;
        public uint AddrAlign;
        public uint EntrySize;
    }

}
