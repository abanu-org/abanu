// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Runtime.InteropServices;
using Mosa.Runtime.x86;

namespace Abanu.Kernel.Core.ConsoleFonts
{

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PSF1Header
    {
        public unsafe fixed byte Ident[2]; //0x36 0x04
        public byte Mode;
        public byte Charsize; //always width=height=charsize
    }

}
