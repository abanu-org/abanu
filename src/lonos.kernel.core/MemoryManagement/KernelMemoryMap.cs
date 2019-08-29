using lonos.Kernel.Core.Boot;
using System;

namespace lonos.Kernel.Core.MemoryManagement
{

    public struct KernelMemoryMap
    {
        public Addr Start;
        public USize Size;
        public BootInfoMemoryType Type;

        public static readonly KernelMemoryMap Empty;

        public KernelMemoryMap(Addr start, USize size, BootInfoMemoryType type)
        {
            Start = start;
            Size = size;
            Type = type;
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
    }

    public static unsafe class KernelMemoryMapManager
    {

        static KernelMemoryMap InitialMap;
        public static KernelMemoryMapHeader* Header;

        public static void Setup()
        {
            var addr = Initial_FindIFreePage();

            KernelMessage.Path("KernelMemoryMapManager", "Initial Page: {0:X}", addr);

            // 80KB should be enough
            // TODO: Check if really 80KB are available after this address.
            InitialMap = new KernelMemoryMap(addr, 0x1000 * 20, BootInfoMemoryType.KernelMemoryMap);
            Memory.InitialKernelProtect_MakeWritable_BySize(InitialMap.Start, InitialMap.Size);

            Header = (KernelMemoryMapHeader*)InitialMap.Start;

            var arrayOffset1 = 0x1000;
            var arrayOffset2 = 0x3000;

            Header->SystemUsable = new KernelMemoryMapArray((KernelMemoryMap*)(InitialMap.Start + arrayOffset1), 50);
            Header->Used = new KernelMemoryMapArray((KernelMemoryMap*)(InitialMap.Start + arrayOffset2), 100);

            for (uint i = 0; i < BootInfo.Header->MemoryMapLength; i++)
            {
                var map = BootInfo.Header->MemoryMapArray[i];
                var kmap = new KernelMemoryMap(map.Start, map.Size, map.Type);
                if (kmap.Type == BootInfoMemoryType.SystemUsable)
                    Header->SystemUsable.Add(kmap);
                else
                {
                    Header->Used.Add(kmap);
                }
            }
            Header->Used.Add(InitialMap);
            //Debug_FillAvailableMemory();
        }

        public static void Debug_FillAvailableMemory()
        {
            uint max = (128 * 1024 * 1024);
            for (uint addr = 0; addr < max; addr += 4)
            {
                if (!Header->Used.Contains(addr) && Header->SystemUsable.Contains(addr))
                {
                    var ptr = (uint*)addr;
                    ptr[0] = 0x11111111;
                }
            }

        }

        static Addr Initial_FindIFreePage()
        {
            for (uint i = 0; i < BootInfo.Header->MemoryMapLength; i++)
            {
                var map = BootInfo.Header->MemoryMapArray[i];
                var usable = Initial_CheckPageIsUsableAfterMap(map);
                if (usable)
                {
                    return map.Start + map.Size;
                }
            }
            return Addr.Zero;
        }

        static bool Initial_CheckPageIsUsableAfterMap(BootInfoMemory map)
        {
            if (map.Type == BootInfoMemoryType.SystemUsable)
                return false;

            Addr checkAddr = map.Start + map.Size;

            var inUsableSystemMap = false;

            for (uint i = 0; i < BootInfo.Header->MemoryMapLength; i++)
            {
                if (BootInfo.Header->MemoryMapArray[i].Type == BootInfoMemoryType.SystemUsable)
                {
                    if (AddressInMap(checkAddr, BootInfo.Header->MemoryMapArray[i]))
                    {
                        inUsableSystemMap = true;
                    }
                }
                else
                {
                    if (AddressInMap(checkAddr, BootInfo.Header->MemoryMapArray[i]))
                    {
                        return false;
                    }
                }

            }
            return inUsableSystemMap;
        }

        static bool AddressInMap(Addr addr, BootInfoMemory map)
        {
            return map.Start <= addr && addr < map.Start + map.Size;
        }

        public static KernelMemoryMap Allocate(USize size, BootInfoMemoryType type)
        {
            var cnt = Header->Used.Count;
            for (uint i = 0; i < cnt; i++)
            {
                var map = Header->Used.Items[i];
                if (CheckPageIsUsableAfterMap(map, size))
                {
                    var newMap = new KernelMemoryMap(map.Start + map.Size, size, type);
                    Header->Used.Add(newMap);
                    KernelMessage.Path("KernelMemoryMapManager", "Allocated: at {0:X8}, size {1:X8}, type {2}", newMap.Start, size, (uint)type);
                    return newMap;
                }
            }
            return KernelMemoryMap.Empty;
        }

        static bool CheckPageIsUsableAfterMap(KernelMemoryMap map, USize size)
        {
            var tryMap = new KernelMemoryMap(map.Start + map.Size, size, BootInfoMemoryType.Unknown);
            if (Header->Used.Intersects(tryMap))
                return false;

            if (!Header->SystemUsable.Contains(tryMap))
                return false;

            return true;
        }

    }
}
