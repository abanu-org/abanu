// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Runtime.InteropServices;
using Mosa.Runtime.x64;

namespace Lonos.Kernel.Core.ConsoleFonts
{

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PSF1Header
    {
        public unsafe fixed byte Ident[2]; //0x36 0x04
        public byte Mode;
        public byte Charsize; //always width=height=charsize
    }

}
