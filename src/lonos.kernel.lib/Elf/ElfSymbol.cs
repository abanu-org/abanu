using System;
using Mosa.Runtime.x86;
using System.Runtime.InteropServices;

namespace lonos.Kernel.Core.Elf
{

    [StructLayout(LayoutKind.Explicit)]
    public struct ElfSymbol
    {
        [FieldOffset(0)] public uint Name;
        [FieldOffset(4)] public uint Value;
        [FieldOffset(8)] public uint Size;
        [FieldOffset(12)] public byte Info;
        [FieldOffset(13)] public byte Other;
        [FieldOffset(14)] public ushort ShNdx;
    }

}
