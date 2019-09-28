// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

namespace Lonos.Kernel.Core.MemoryManagement
{
    public static unsafe class PhysicalPageManager
    {

        private static IPageFrameAllocator Default;
        public const uint PageSize = 4096;

        public static void Setup()
        {
            var allocator = new PhysicalPageAllocator();
            allocator.Initialize();
            Default = allocator;
        }

        public static Page* AllocatePages(uint pages)
        {
            return Default.AllocatePages(pages);
        }

        public static Page* AllocatePage()
        {
            var p = Default.AllocatePage();
            //if (p->PhysicalAddress == 0x01CA4000)
            //    Panic.Error("DEBUG-MARKER");
            return p;
        }

        public static Addr AllocatePagesAddr(uint pages)
        {
            return GetAddress(AllocatePages(pages));
        }

        public static Addr AllocatePageAddr()
        {
            return GetAddress(AllocatePage());
        }

        public static MemoryRegion AllocateRegion(USize size)
        {
            var pages = KMath.DivCeil(size, 4096);
            var p = AllocatePages(pages);
            return new MemoryRegion(Default.GetAddress(p), pages * 4096);
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

        public static Page* GetPageByIndex(uint pageIndex)
        {
            return Default.GetPageByIndex(pageIndex);
        }

        public static uint PagesAvailable
        {
            get
            {
                return Default.FreePages;
            }
        }

        public static uint GetAddress(Page* page)
        {
            return Default.GetAddress(page);
        }

        public static Page* NextPage(Page* page)
        {
            return page + 1;
        }

    }
}
