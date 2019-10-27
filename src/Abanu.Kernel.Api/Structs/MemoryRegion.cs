// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Runtime.Versioning;

namespace Abanu.Kernel.Core
{

    [Serializable]
    public struct MemoryRegion
    {
        public Addr Start;
        public USize Size;

        public MemoryRegion(Addr start, USize size)
        {
            Start = start;
            Size = size;
        }

        public static readonly MemoryRegion Zero;
        public static readonly MemoryRegion Invalid = new MemoryRegion(0xFFFFFFFE, 0);

        public bool Contains(Addr addr)
        {
            return Start <= addr && addr < Start + Size;
        }

        public static MemoryRegion FromLocation(Addr start, Addr end)
        {
            return new MemoryRegion(start, end - start);
        }

        public MemoryRegion SubRegion(USize offset)
        {
            return new MemoryRegion(Start + offset, Size - offset);
        }

        public MemoryRegion SubRegion(USize offset, USize length)
        {

            var reg = new MemoryRegion(Start + offset, length - offset);
            Assert.False(reg.Start + reg.Size > Start + Size, "MemoryRegion::SubRegion: Argument Exception");
            return reg;
        }

        public MemoryRegion TrimEndLocation(Addr addr)
        {
            return FromLocation(Start, addr);
        }

    }

    public unsafe struct LinkedMemoryRegion
    {
        public LinkedMemoryRegion(MemoryRegion region)
        {
            Region = region;
            Next = null;
        }

        public LinkedMemoryRegion(MemoryRegion region, LinkedMemoryRegion* appendTo)
        {
            Region = region;
            fixed (LinkedMemoryRegion* ptr = &this)
            {
                appendTo->Next = ptr;
            }
        }

        public MemoryRegion Region;
        public LinkedMemoryRegion* Next;
    }

}
