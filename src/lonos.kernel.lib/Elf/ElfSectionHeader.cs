using System;
using Mosa.Runtime.x86;
using System.Runtime.InteropServices;

namespace lonos.Kernel.Core.Elf
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
