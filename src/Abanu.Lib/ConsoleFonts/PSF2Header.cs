// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Runtime.InteropServices;
using Mosa.Runtime.x86;

namespace Abanu.Kernel.Core.ConsoleFonts
{

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PSF2Header
    {
        public unsafe fixed byte Ident[4]; //0x72 0xb5 0x4a 0x86
        public uint Version;
        public uint Headersize;
        public uint Flags;
        public uint Length; // number of glyphs
        public uint Charsize;
        public uint Height;
        public uint Width;
    }

}
