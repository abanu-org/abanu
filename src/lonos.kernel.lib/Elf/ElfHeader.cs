using System;
using Mosa.Runtime.x86;
using System.Runtime.InteropServices;

namespace lonos.kernel.core
{

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ElfHeader
    {
        public static uint Magic1 = 0x464c457f; //0x7f + "ELF"

        public unsafe fixed uint Ident[4];
        public ushort Type;
        public ushort Machine;
        public uint Version;
        public uint Entry;
        public uint PhOff;
        public uint ShOff;
        public uint Flags;
        public ushort EhSize;
        public ushort PhEntSize;
        public ushort PhNum;
        public ushort ShEntSize;
        public ushort ShNum;
        public ushort ShStrNdx;
    }

}
