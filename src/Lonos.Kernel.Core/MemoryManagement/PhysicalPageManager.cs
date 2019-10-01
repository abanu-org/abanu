// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using Lonos.Kernel.Core.Boot;
using Lonos.Kernel.Core.PageManagement;

namespace Lonos.Kernel.Core.MemoryManagement
{
    public static unsafe class PhysicalPageManager
    {

        private static IPageFrameAllocator Default;
        public const uint PageSize = 4096;

        public static void Setup()
        {
            var allocator = new PhysicalInitialPageAllocator();
            //allocator.Setup(new MemoryRegion(2 * 1024 * 1024, BootInfo.Header->InstalledPhysicalMemory - (2 * 1024 * 1024)), AddressSpaceKind.Physical);
            allocator.Setup(new MemoryRegion(0, BootInfo.Header->InstalledPhysicalMemory), AddressSpaceKind.Physical);
            Default = allocator;

            ClearKernelReserved();
            SelfTest();
        }

        private static void ClearKernelReserved()
        {
            // TODO: Determine dynamically
            //ClearRegionsOfInterest(0x0, 640 * 1024);
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

        private const bool SelfTestDump = true;

        public static void SelfTest()
        {
            if (SelfTestDump)
                Default.Dump();

            KernelMessage.WriteLine("Begin SelfTest");

            var ptrPages = ((BootInfo.Header->InstalledPhysicalMemory / 4096) * 4) / 4096;
            var ptrList = (Addr*)AllocatePageAddr(ptrPages); // pointers for 4GB of pages
            var ptrListMapped = (Addr*)0x3000;
            PageTable.KernelTable.Map(ptrListMapped, ptrList, ptrPages * 4096, true, true);
            var checkPageCount = Default.FreePages;
            checkPageCount -= 1000;
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
                    *mapPtr = 0xEEEEEEEE;
                    mapPtr += 1;
                }
                PageTable.KernelTable.UnMap(mapAddr, 4096, true);
                //Default.Free(testPage);
            }

            if (SelfTestDump)
                Default.Dump();

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
                Default.Dump();
                KernelMessage.WriteLine("Final Dump");
            }
        }

        public static Page* AllocatePages(uint pages, AllocatePageOptions options = AllocatePageOptions.Default)
        {
            return Default.AllocatePages(pages);
        }

        public static Page* AllocatePage(AllocatePageOptions options = AllocatePageOptions.Default)
        {
            var p = Default.AllocatePage();
            //if (p->PhysicalAddress == 0x01CA4000)
            //    Panic.Error("DEBUG-MARKER");
            return p;
        }

        public static Addr AllocatePageAddr(uint pages, AllocatePageOptions options = AllocatePageOptions.Default)
        {
            return Default.AllocatePagesAddr(pages, options);
        }

        public static Addr AllocatePageAddr(AllocatePageOptions options = AllocatePageOptions.Default)
        {
            return Default.AllocatePageAddr(options);
        }

        public static MemoryRegion AllocateRegion(USize size, AllocatePageOptions options = AllocatePageOptions.Default)
        {
            return Default.AllocateRegion(size, options);
        }

        public static void Free(Page* page)
        {
            Default.Free(page);
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

        public static Addr GetAddress(Page* page)
        {
            return Default.GetAddress(page);
        }

        public static Page* NextPage(Page* page)
        {
            return Default.NextPage(page);
        }

    }
}
