using System;
using Mosa.Runtime;

//using Mosa.Kernel.x86;


namespace lonos.kernel.core
{

    /// <summary>
    /// A physical page allocator.
    /// </summary>
    public unsafe static class PageFrameAllocator
    {

        static uint PagesUsed;

        static Page* PageArray;
        static uint PageCount;

        static KernelMemoryMap kmap;

        /// <summary>
        /// Setup the physical page manager
        /// </summary>
        public static void Setup()
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
                    PageArray[i-1].Next = &PageArray[i];
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
        unsafe static void SetupFreeMemory()
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

        public static Page* GetPage(Addr addr)
        {
            return &PageArray[(uint)addr / PageSize];
        }

        //static uint _nextAllocacationSearchIndex;
        private static Page* lastAllocatedPage;

        /// <summary>
        /// Allocate a physical page from the free list
        /// </summary>
        /// <returns>The page</returns>
        public static Addr Allocate()
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

            //for (uint i = _nextAllocacationSearchIndex; i < PageCount; i++)
            //{
            //    if (PageArray[i].Free)
            //    {
            //        p = &PageArray[i];
            //        _nextAllocacationSearchIndex = i + 1;
            //    }
            //}
            //for (uint i = 0; i < _nextAllocacationSearchIndex; i++)
            //{
            //    if (PageArray[i].Free)
            //    {
            //        p = &PageArray[i];
            //        _nextAllocacationSearchIndex = i + 1;
            //    }
            //}
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
        public static void Free(IntPtr address)
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
        public static uint PageSize { get { return 4096; } }

        public static uint PagesAvailable
        {
            get
            {
                return PageCount - PagesUsed;
            }
        }

    }
}
