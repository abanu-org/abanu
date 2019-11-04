// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using Abanu.Kernel.Core.Boot;
using Abanu.Kernel.Core.MemoryManagement.PageAllocators;
using Abanu.Kernel.Core.PageManagement;

namespace Abanu.Kernel.Core.MemoryManagement
{
    public static unsafe class PhysicalPageManager
    {

        private static IPageFrameAllocator Default;
        public const uint PageSize = 4096;

        public static void Setup()
        {
            var allocator = new PhysicalInitialPageAllocator() { DebugName = "PhysInitial" };
            //allocator.Setup(new MemoryRegion(2 * 1024 * 1024, BootInfo.Header->InstalledPhysicalMemory - (2 * 1024 * 1024)), AddressSpaceKind.Physical);
            allocator.Setup(new MemoryRegion(0, BootInfo.Header->InstalledPhysicalMemory).TrimEndLocation(Address.IdentityMapStart), AddressSpaceKind.Physical);
            Default = allocator;

            ClearKernelReserved();
            //UnmapUnsedPages();
            SelfTest();
        }

        //private static void UnmapUnsedPages()
        //{
        //    KernelMessage.WriteLine("Unmap unused pages...");
        //    var maxVirtPages = Address.MaximumMemory / 4096;
        //    for (uint pageNum = 0; pageNum < maxVirtPages; pageNum++)
        //    {
        //        var addr = pageNum * 4096;
        //        if (!KernelMemoryMapManager.Header->Used.ContainsVirtual(addr))
        //        {
        //            //KernelMessage.WriteLine("{0:X8}", addr);
        //            PageTable.KernelTable.UnMap(addr, true); //TODO: Flush later
        //        }
        //    }
        //}

        private static void ClearKernelReserved()
        {
            for (var mapIdx = 0; mapIdx < KernelMemoryMapManager.Header->KernelReserved.Count; mapIdx++)
            {
                var map = &KernelMemoryMapManager.Header->KernelReserved.Items[mapIdx];
                if ((map->AddressSpaceKind & AddressSpaceKind.Physical) == 0)
                    continue;

                KernelMessage.WriteLine("Clear kernel reserved region {0:X8}-{1:X8}, Size {2:X8}, Skipping Used Regions.", map->Start, map->Start + map->Size - 1, map->Size);

                var pageStart = map->Start;
                while (pageStart < map->Start + map->Size)
                {
                    if (!KernelMemoryMapManager.Header->Used.Contains(pageStart))
                        ClearKernelReserved(pageStart);
                    pageStart += 4096;
                }
            }
        }

        private static void ClearKernelReserved(Addr physPageStart)
        {
            if (SelfTestDump)
                KernelMessage.WriteLine("Clear out at {0:X8}", physPageStart);
            var mapAddr = 0x2000u;
            PageTable.KernelTable.Map(mapAddr, physPageStart, 4096, true, true);
            var mapPtr = (uint*)mapAddr;
            for (var pos = 0; pos < 4096 / 4; pos++)
            {
                *mapPtr = 0xFFFFFFFF;
                mapPtr += 1;
            }
            PageTable.KernelTable.UnMap(mapAddr, 4096, true);
        }

        private const bool SelfTestDump = false;

        public static void SelfTest()
        {
            if (SelfTestDump)
                Default.DumpPages();

            KernelMessage.WriteLine("Begin SelfTest");

            var ptrPages = ((BootInfo.Header->InstalledPhysicalMemory / 4096) * 4) / 4096;
            var ptrList = (Addr*)AllocatePageAddr(ptrPages); // pointers for 4GB of pages
            var ptrListMapped = (Addr*)0x3000;
            PageTable.KernelTable.Map(ptrListMapped, ptrList, ptrPages * 4096, true, true);
            var checkPageCount = Default.FreePages;
            checkPageCount -= Default.CriticalLowPages;
            //checkPageCount = 32;
            var mapAddr = 0x2000u;
            for (var i = 0; i < checkPageCount; i++)
            {
                if (SelfTestDump)
                    KernelMessage.Write(".");
                var testPage = Default.AllocatePage();
                var testAddr = Default.GetAddress(testPage);
                ptrListMapped[i] = testAddr;
                PageTable.KernelTable.Map(mapAddr, testAddr, 4096, true, true);
                var mapPtr = (uint*)mapAddr;
                for (var pos = 0; pos < 1024; pos++)
                {
                    *mapPtr = 0xEBFEEBFE;
                    mapPtr += 1;
                }
                PageTable.KernelTable.UnMap(mapAddr, 4096, true);
                //Default.Free(testPage);
            }

            if (SelfTestDump)
                Default.DumpPages();

            KernelMessage.WriteLine("Free Pages now");
            for (var i = 0; i < checkPageCount; i++)
            {
                if (SelfTestDump)
                    KernelMessage.Write(":");
                var testAddr = ptrListMapped[i];
                Default.FreeAddr(testAddr);
            }
            Default.FreeAddr(ptrList);

            KernelMessage.WriteLine("SelfTest Done");
            if (SelfTestDump)
            {
                Default.DumpPages();
                KernelMessage.WriteLine("Final Dump");
            }
        }

        public static Page* AllocatePages(uint pages, AllocatePageOptions options = default)
        {
            return Default.AllocatePages(pages, options);
        }

        public static Page* AllocatePage(AllocatePageOptions options = default)
        {
            var p = Default.AllocatePage(options);
            //if (p->PhysicalAddress == 0x01CA4000)
            //    Panic.Error("DEBUG-MARKER");
            return p;
        }

        public static Addr AllocatePageAddr(uint pages, AllocatePageOptions options = default)
        {
            return Default.AllocatePagesAddr(pages, options);
        }

        public static Addr AllocatePageAddr(AllocatePageOptions options = default)
        {
            return Default.AllocatePageAddr(options);
        }

        public static MemoryRegion AllocateRegion(USize size, AllocatePageOptions options = default)
        {
            return Default.AllocateRegion(size, options);
        }

        public static void Free(Page* page)
        {
            Default.Free(page);
        }

        public static void FreeAddr(Addr addr)
        {
            Default.FreeAddr(addr);
        }

        public static Page* GetPhysPage(Addr physAddr)
        {
            return Default.GetPageByAddress(physAddr);
        }

        public static Page* GetPageByNum(uint pageNum)
        {
            return Default.GetPageByNum(pageNum);
        }

        public static uint TotalPages
        {
            get
            {
                return Default.TotalPages;
            }
        }

        public static uint FreePages
        {
            get
            {
                return Default.FreePages;
            }
        }

        public static Addr GetAddress(Page* page)
        {
            return Default.GetAddress(page);
        }

        public static Page* NextPage(Page* page)
        {
            return Default.NextPage(page);
        }

        public static Page* NextCompoundPage(Page* page)
        {
            return Default.NextCompoundPage(page);
        }

        public static void SetTraceOptions(PageFrameAllocatorTraceOptions options)
        {
            Default.SetTraceOptions(options);
        }

        public static void DumpStats()
        {
            Default.DumpStats();
        }

        public static void DumpPages()
        {
            Default.DumpPages();
        }

        public static IPageFrameAllocator GetAllocatorByAddr(Addr addr) => Default;

    }
}
