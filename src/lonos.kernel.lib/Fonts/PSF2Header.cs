using System;
using Mosa.Runtime.x86;
using System.Runtime.InteropServices;

namespace lonos.kernel.core
{

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PSF2Header
    {
        public unsafe fixed byte Ident[4]; //0x72 0xb5 0x4a 0x86
        public uint version;
        public uint headersize;
        public uint flags;
        public uint length; //number of glyphs
        public uint charsize;
        public uint height, width;
    }

}