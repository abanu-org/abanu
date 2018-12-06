using System;
using Mosa.Runtime;

//using Mosa.Kernel.x86;


namespace lonos.kernel.core
{


    public unsafe static class PageFrameManager
    {

        static PageFrameAllocator Default;

        public static void Setup()
        {
            Default = new PageFrameAllocator();
            Default.Setup();
        }

        public static Page* AllocatePages(PageFrameRequestFlags flags, byte order)
        {
            return Default.AllocatePages(flags, order); 
        }

        public static Page* AllocatePage(PageFrameRequestFlags flags)
        {
            return Default.AllocatePage(flags);
        }

        public static void Free(Page* page)
        {
            Default.Free(page);
        }

        public static uint PagesAvailable
        {
            get
            {
                return Default.PagesAvailable;
            }
        }

    }

    /// <summary>
    /// A physical page allocator.
    /// </summary>
    public unsafe class PageFrameAllocator : IPageFrameAllocator
    {

        public Page* AllocatePages(PageFrameRequestFlags flags, byte order)
        {
            int size = 1 << order;
            var page = GetPage(Allocate());
            for (var i = 1; i < size; i++)
            {
                Allocate();
            }
            return page;
        }

        public Page* AllocatePage(PageFrameRequestFlags flags)
        {
            // Fake-Impl
            return GetPage(Allocate());
        }

        public void Free(Page* page)
        {
            Free(page->PhysicalAddress);
        }

        uint PagesUsed;

        Page* PageArray;
        uint PageCount;

        KernelMemoryMap kmap;

        /// <summary>
        /// Setup the physical page manager
        /// </summary>
        public void Setup()
        {
            uint physMem = BootInfo.Header->InstalledPhysicalMemory;
            PageCount = physMem / PageSize;
            kmap = KernelMemoryMapManager.Allocate(PageCount * (uint)sizeof(Page), BootInfoMemoryType.PageFrameAllocator);
            PageArray = (Page*)kmap.Start;
            lastAllocatedPage = PageArray;

            MemoryOperation.Clear4(kmap.Start, kmap.Size);

            for (uint i = 0; i < PageCount; i++)
            {
                PageArray[i].PhysicalAddress = i * PageSize;
                if (i != 0)
                    PageArray[i - 1].Next = &PageArray[i];
            }

            SetupFreeMemory();

            for (uint i = 0; i < PageCount; i++)
            {
                if (!PageArray[i].Used)
                    PageArray[i].Status = PageStatus.Free;
            }
        }

        /// <summary>
        /// Setups the free memory.
        /// </summary>
        unsafe void SetupFreeMemory()
        {
            if (!BootInfo.Present)
                return;

            for (var i = 0; i < KernelMemoryMapManager.Header->Used.Count; i++)
            {
                var map = KernelMemoryMapManager.Header->Used.Items[i];
                GetPage(map.Start)->Status = PageStatus.Used;
                PagesUsed++;
            }
        }

        public Page* GetPage(Addr addr)
        {
            return &PageArray[(uint)addr / PageSize];
        }

        //static uint _nextAllocacationSearchIndex;
        private static Page* lastAllocatedPage;

        /// <summary>
        /// Allocate a physical page from the free list
        /// </summary>
        /// <returns>The page</returns>
        Addr Allocate()
        {
            var cnt = 0;
            Page* p = lastAllocatedPage->Next;
            while (true)
            {
                if (p == null)
                    p = PageArray;
                if (p->Status == PageStatus.Free)
                    break;
                p = p->Next;
                if (++cnt > PageCount)
                    break;
            }

            if (p == null || p->Status != PageStatus.Free)
            {
                return Addr.Invalid;
            }

            p->Status = PageStatus.Used;
            PagesUsed++;
            lastAllocatedPage = p;
            return p->PhysicalAddress;
        }

        /// <summary>
        /// Releases a page to the free list
        /// </summary>
        /// <param name="address">The address.</param>
        void Free(Addr address)
        {
            var p = GetPage(address);
            if (p->Free)
                return;
            p->Status = PageStatus.Free;
            PagesUsed--;
        }

        /// <summary>
        /// Retrieves the size of a single memory page.
        /// </summary>
        public uint PageSize { get { return 4096; } }

        public uint PagesAvailable
        {
            get
            {
                return PageCount - PagesUsed;
            }
        }

    }
}
