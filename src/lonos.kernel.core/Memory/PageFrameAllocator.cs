using System;
using Mosa.Runtime;

//using Mosa.Kernel.x86;


namespace lonos.kernel.core
{


    public unsafe static class PageFrameManager
    {

        static PageFrameAllocator Default;
        public const uint PageSize = 4096;

        public static void Setup()
        {
            Default = new PageFrameAllocator();
            Default.Setup();
        }

        public static Page* AllocatePages(PageFrameRequestFlags flags, uint pages)
        {
            return Default.AllocatePages(flags, pages);
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

        public Page* AllocatePages(PageFrameRequestFlags flags, uint pages)
        {
            return Allocate(pages);
        }

        public Page* AllocatePage(PageFrameRequestFlags flags)
        {
            return GetPhysPage(Allocate(1));
        }

        public void Free(Page* page)
        {
            Free(page->PhysicalAddress);
        }

        uint FreePages;

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

            Memory.InitialKernelProtect_MakeWritable_BySize(kmap.Start, kmap.Size);
            MemoryOperation.Clear4(kmap.Start, kmap.Size);

            for (uint i = 0; i < PageCount; i++)
            {
                PageArray[i].PhysicalAddress = i * PageSize;
                if (i != 0)
                    PageArray[i - 1].Next = &PageArray[i];
            }

            SetupFreeMemory();
        }

        /// <summary>
        /// Setups the free memory.
        /// </summary>
        unsafe void SetupFreeMemory()
        {
            if (!BootInfo.Present)
                return;

            for (uint i = 0; i < PageCount; i++)
                PageArray[i].Status = PageStatus.Reserved;

            SetInitialPageStatus(&KernelMemoryMapManager.Header->SystemUsable, PageStatus.Free);
            SetInitialPageStatus(&KernelMemoryMapManager.Header->Used, PageStatus.Used);

            FreePages = 0;
            for (uint i = 0; i < PageCount; i++)
                if (PageArray[i].Status == PageStatus.Free)
                    FreePages++;

            KernelMessage.WriteLine("Pages Free: {0}", FreePages);
        }

        private void SetInitialPageStatus(KernelMemoryMapArray* maps, PageStatus status)
        {
            for (var i = 0; i < maps->Count; i++)
            {
                var map = maps->Items[i];
                if (map.Start >= BootInfo.Header->InstalledPhysicalMemory)
                    continue;

                var mapPages = KMath.DivCeil(map.Size, 4096);
                KernelMessage.WriteLine("Mark Pages from {0:X8}, Size {1:X8}, Pages {2}, Type {3}", map.Start, map.Size, mapPages, (uint)status);

                for (var p = 0; p < mapPages; p++)
                {
                    var addr = map.Start + p * 4096;
                    if (addr >= BootInfo.Header->InstalledPhysicalMemory)
                        break;
                    GetPhysPage(addr)->Status = status;
                }
            }
        }

        public void Dump()
        {
            var sb = new StringBuffer();
            for (uint i = 0; i < PageCount; i++)
            {
                var p = &PageArray[i];
                sb.Clear();
                sb.Append("Page {0} at {1:X8}: {2}\n", p->PageNum, p->PhysicalAddress, (uint)p->Status);
                sb.WriteTo(Devices.Serial1);
            }
        }

        public Page* GetPhysPage(Addr addr)
        {
            if (addr >= BootInfo.Header->InstalledPhysicalMemory)
                return null;
            return &PageArray[(uint)addr / PageSize];
        }

        public Page* GetPageByNum(uint pageNum)
        {
            return &PageArray[pageNum];
        }

        //static uint _nextAllocacationSearchIndex;
        private static Page* lastAllocatedPage;

        /// <summary>
        /// Allocate a physical page from the free list
        /// </summary>
        /// <returns>The page</returns>
        Page* Allocate(uint num)
        {
            //KernelMessage.WriteLine("Request {0} pages...", num);

            var cnt = 0;

            if (lastAllocatedPage == null)
                lastAllocatedPage = PageArray;

            Page* p = lastAllocatedPage->Next;
            while (true)
            {
                if (p == null)
                    p = PageArray;

                if (p->Status == PageStatus.Free)
                {
                    var head = p;

                    // Found free Page. Check now free range.
                    for (var i = 0; i < num; i++)
                    {
                        if (p == null)
                            break; // Reached end. SorRange is incomplete
                        if (p->Status != PageStatus.Free) // Used -> so we can abort the searach
                            break;

                        if (i == num - 1)
                        { // all loops successful. So we found our range.

                            head->Tail = p;
                            head->PagesUsed = num;
                            p = head;
                            for (var n = 0; n < num; n++)
                            {
                                p->Status = PageStatus.Used;
                                p->Head = head;
                                p->Tail = head->Tail;
                                p = p->Next;
                                FreePages--;
                            }
                            lastAllocatedPage = p;

                            //KernelMessage.WriteLine("Allocated from {0:X8} to {1:X8}", (uint)head->PhysicalAddress, (uint)head->Tail->PhysicalAddress + 4096 - 1);

                            return head;
                        }

                        p = p->Next;
                    }

                }

                if (p->Tail != null)
                    p = p->Tail;

                p = p->Next;
                if (++cnt > PageCount)
                    break;
            }

            Panic.Error("PageFrameAllocator: No free Page found");
            return null;
        }

        /// <summary>
        /// Releases a page to the free list
        /// </summary>
        void Free(Addr address)
        {
            var p = GetPhysPage(address);
            if (p->Free)
                return;

            var num = p->PagesUsed;

            for (var n = 0; n < num; n++)
            {
                p->Status = PageStatus.Used;
                p->PagesUsed = 0;
                p->Head = null;
                p->Tail = null;
                p = p->Next;
                FreePages++;
            }
        }

        /// <summary>
        /// Retrieves the size of a single memory page.
        /// </summary>
        public uint PageSize { get { return 4096; } }

        public uint PagesAvailable
        {
            get
            {
                return FreePages;
            }
        }

    }
}
