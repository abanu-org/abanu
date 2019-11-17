// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System.Runtime.InteropServices;

namespace Abanu.Kernel.Core.Elf
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ElfProgramHeader
    {
        public uint Type;
        public uint Offset;
        public uint VAddr;
        public uint PAddr;
        public uint FileSz;
        public uint MemSz;
        public uint Flags;
        public uint Align;
    }

}
