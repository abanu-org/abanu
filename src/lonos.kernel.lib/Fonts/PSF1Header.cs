

using System;
using Mosa.Runtime.x86;
using System.Runtime.InteropServices;

namespace lonos.kernel.core
{

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PSF1Header
    {
        public unsafe fixed byte Ident[2]; //0x36 0x04
        public byte mode;
        public byte charsize; //always width=height=charsize
    }

}