// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using Lonos.Kernel.Core.Boot;

namespace Lonos.Kernel.Core.MemoryManagement
{

    public struct KernelMemoryMap
    {
        public Addr Start;
        public USize Size;
        public BootInfoMemoryType Type;

        /// <summary>
        /// Specifies the address space kind of <see cref="Start"/>
        /// </summary>
        public AddressSpaceKind AddressSpaceKind;

        public static readonly KernelMemoryMap Empty;

        public KernelMemoryMap(Addr start, USize size, BootInfoMemoryType type, AddressSpaceKind addressSpaceKind)
        {
            Start = start;
            Size = size;
            Type = type;
            AddressSpaceKind = addressSpaceKind;
        }

        public bool ContainsAddr(Addr addr)
        {
            return Start <= addr && addr < Start + Size;
        }

        public bool ContainsMap(KernelMemoryMap map)
        {
            return Start <= map.Start && map.Start + map.Size < Start + Size;
        }

        public bool Intersects(KernelMemoryMap map)
        {
            return ContainsAddr(map.Start) || ContainsAddr(map.Start + map.Size);
        }

    }

    public unsafe struct KernelMemoryMapArray
    {
        public KernelMemoryMap* Items;
        public uint Count;
        private uint Reserved;

        public KernelMemoryMapArray(KernelMemoryMap* items, uint reserved)
        {
            Items = items;
            Reserved = reserved;
            Count = 0;
        }

        public void Add(KernelMemoryMap map)
        {
            Assert.True(HasCapacity);

            Items[Count] = map;
            Count++;
        }

        public bool Intersects(Addr addr)
        {
            for (var i = 0; i < Count; i++)
            {
                if (Items[i].ContainsAddr(addr))
                    return true;
            }
            return false;
        }

        public bool Intersects(KernelMemoryMap map)
        {
            for (var i = 0; i < Count; i++)
            {
                if (Items[i].Intersects(map))
                    return true;
            }
            return false;
        }

        public bool Contains(KernelMemoryMap map)
        {
            for (var i = 0; i < Count; i++)
            {
                if (Items[i].ContainsMap(map))
                    return true;
            }
            return false;
        }

        public bool Contains(Addr addr)
        {
            for (var i = 0; i < Count; i++)
            {
                if (Items[i].ContainsAddr(addr))
                    return true;
            }
            return false;
        }

        private bool HasCapacity
        {
            get
            {
                return Count < Reserved;
            }
        }
    }

    public struct KernelMemoryMapHeader
    {
        public KernelMemoryMapArray Used;
        public KernelMemoryMapArray SystemUsable;
        public KernelMemoryMapArray KernelReserved;
    }
}
