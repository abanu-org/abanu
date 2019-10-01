// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using Lonos.Kernel.Core.Boot;
using Lonos.Kernel.Core.PageManagement;

namespace Lonos.Kernel.Core.MemoryManagement
{
    public static unsafe class KernelMemoryMapManager
    {

        private static KernelMemoryMap InitialMap;
        public static KernelMemoryMapHeader* Header;

        public static void Setup()
        {
            var addr = Initial_FindFreePage();

            KernelMessage.Path("KernelMemoryMapManager", "Initial Page: {0:X}", addr);

            // 80KB should be enough
            // TODO: Check if really 80KB are available after this address.
            InitialMap = new KernelMemoryMap(addr, 0x1000 * 20, BootInfoMemoryType.KernelMemoryMap, AddressSpaceKind.Both);
            PageTableExtensions.SetWritable(PageTable.KernelTable, InitialMap.Start, InitialMap.Size);

            Header = (KernelMemoryMapHeader*)InitialMap.Start;

            var arrayOffset1 = 0x1000;
            var arrayOffset2 = 0x2000;
            var arrayOffset3 = 0x3000;

            Header->SystemUsable = new KernelMemoryMapArray((KernelMemoryMap*)(InitialMap.Start + arrayOffset1), 50);
            Header->Used = new KernelMemoryMapArray((KernelMemoryMap*)(InitialMap.Start + arrayOffset2), 100);
            Header->KernelReserved = new KernelMemoryMapArray((KernelMemoryMap*)(InitialMap.Start + arrayOffset3), 100);

            for (uint i = 0; i < BootInfo.Header->MemoryMapLength; i++)
            {
                var map = BootInfo.Header->MemoryMapArray[i];
                var kmap = new KernelMemoryMap(map.Start, map.Size, map.Type, map.AddressSpaceKind);
                if (kmap.Type == BootInfoMemoryType.SystemUsable)
                {
                    Header->SystemUsable.Add(kmap);
                }
                else
                {
                    if (kmap.Type == BootInfoMemoryType.KernelReserved)
                        Header->KernelReserved.Add(kmap);
                    else
                        Header->Used.Add(kmap);
                }
            }
            Header->Used.Add(InitialMap);

            KernelMessage.Path("KernelMemoryMapManager", "Filling Lists Done. SystemUsable: {0}, CustomReserved: {1}, Used: {2}", Header->SystemUsable.Count, Header->KernelReserved.Count, Header->Used.Count);
            PrintMapArray("SytemUsable", &Header->SystemUsable);
            PrintMapArray("CustomReserved", &Header->KernelReserved);
            PrintMapArray("Used", &Header->Used);

            //Debug_FillAvailableMemory();
        }

        private static void PrintMapArray(string name, KernelMemoryMapArray* mapArray)
        {
            KernelMessage.WriteLine("Items of MemoryMap Array [{0}]", name);
            for (var i = 0; i < mapArray->Count; i++)
            {
                var mm = &mapArray->Items[i];
                KernelMessage.WriteLine("Map Start={0:X8}, Size={1:X8}, Type={2}, AddrKind={3}", mm->Start, mm->Size, (uint)mm->Type, (uint)mm->AddressSpaceKind);
            }
        }

        public static void Debug_FillAvailableMemory()
        {
            uint max = 256 * 1024 * 1024;
            for (uint addr = 0; addr < max; addr += 4)
            {
                if (!Header->Used.Contains(addr) && Header->SystemUsable.Contains(addr))
                {
                    var ptr = (uint*)addr;
                    ptr[0] = 0x11111111;
                }
            }

        }

        private static Addr Initial_FindFreePage()
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

        private static bool Initial_CheckPageIsUsableAfterMap(BootInfoMemory map)
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
                        inUsableSystemMap = true;
                }
                else
                {
                    if (AddressInMap(checkAddr, BootInfo.Header->MemoryMapArray[i]))
                        return false;
                }

            }
            return inUsableSystemMap;
        }

        private static bool AddressInMap(Addr addr, BootInfoMemory map)
        {
            return map.Start <= addr && addr < map.Start + map.Size;
        }

        public static KernelMemoryMap Allocate(USize size, BootInfoMemoryType type, AddressSpaceKind addressSpaceKind)
        {
            var cnt = Header->Used.Count;
            for (uint i = 0; i < cnt; i++)
            {
                var map = Header->Used.Items[i];
                if (CheckPageIsUsableAfterMap(map, size, addressSpaceKind))
                {
                    var newMap = new KernelMemoryMap(map.Start + map.Size, size, type, addressSpaceKind);
                    Header->Used.Add(newMap);
                    KernelMessage.Path("KernelMemoryMapManager", "Allocated: at {0:X8}, size {1:X8}, type {2}", newMap.Start, size, (uint)type);
                    return newMap;
                }
            }
            return KernelMemoryMap.Empty;
        }

        private static bool CheckPageIsUsableAfterMap(KernelMemoryMap map, USize size, AddressSpaceKind addressSpaceKind)
        {
            var tryMap = new KernelMemoryMap(map.Start + map.Size, size, BootInfoMemoryType.Unknown, addressSpaceKind);
            if (Header->Used.Intersects(tryMap))
                return false;

            if (Header->KernelReserved.Intersects(tryMap))
                return false;

            if (!Header->SystemUsable.Contains(tryMap))
                return false;

            return true;
        }

    }
}
