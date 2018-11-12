using System;
using Mosa.Runtime.x86;
using System.Runtime.InteropServices;

namespace lonos.kernel.core
{

    [StructLayout(LayoutKind.Explicit)]
    public struct ElfHeader
    {
        public static uint Magic1 = 0x464c457f; //0x7f + "ELF"

        [FieldOffset(0)] public uint Ident1;
        [FieldOffset(4)] public uint Ident2;
        [FieldOffset(8)] public uint Ident3;
        [FieldOffset(12)] public uint Ident4;
        [FieldOffset(16)] public ushort Type;
        [FieldOffset(18)] public ushort Machine;
        [FieldOffset(20)] public uint Version;
        [FieldOffset(24)] public uint Entry;
        [FieldOffset(28)] public uint PhOff;
        [FieldOffset(32)] public uint ShOff;
        [FieldOffset(36)] public uint Flags;
        [FieldOffset(40)] public ushort EhSize;
        [FieldOffset(42)] public ushort PhEntSize;
        [FieldOffset(44)] public ushort PhNum;
        [FieldOffset(46)] public ushort ShEntSize;
        [FieldOffset(48)] public ushort ShNum;
        [FieldOffset(50)] public ushort ShStrNdx;
    }

}
