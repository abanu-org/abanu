using System.Runtime.Versioning;
using System;

namespace lonos.kernel.core
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
