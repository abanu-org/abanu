using System;
using lonos.Kernel.Core.Boot;
using lonos.Kernel.Core.Devices;
using lonos.Kernel.Core.Diagnostics;
using lonos.Kernel.Core.PageManagement;
using Mosa.Runtime;

//using Mosa.Kernel.x86;

namespace lonos.Kernel.Core.MemoryManagement
{

    public static unsafe class PageFrameManager
    {

        private static PageFrameAllocator Default;
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
            var p = Default.AllocatePage(flags);
            //if (p->PhysicalAddress == 0x01CA4000)
            //    Panic.Error("DEBUG-MARKER");
            return p;
        }

        public static void Free(Page* page)
        {
            Default.Free(page);
        }

        public static Page* GetPhysPage(Addr physAddr)
        {
            return Default.GetPhysPage(physAddr);
        }

        public static Page* GetPageByNum(uint pageNum)
        {
            return Default.GetPageByNum(pageNum);
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
            var p = Allocate(1);
            //if (p->PhysicalAddress == 0x01CA4000)
            //    Panic.Error("DEBUG-MARKER");
            return p;
        }

        public void Free(Page* page)
        {
            Free(page->PhysicalAddress);
        }

        private uint FreePages;

        private Page* PageArray;
        private uint PageCount;

        private KernelMemoryMap kmap;

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

            var firstSelfPageNum = KMath.DivFloor(kmap.Start, 4096);
            var selfPages = KMath.DivFloor(kmap.Size, 4096);

            KernelMessage.WriteLine("Page Frame Array allocated {0} pages, beginning with page {1}", selfPages, firstSelfPageNum);

            PageTableExtensions.SetWritable(PageTable.KernelTable, kmap.Start, kmap.Size);
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
        private unsafe void SetupFreeMemory()
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

            //if (GetPhysPage(0x01CA3000)->Status != PageStatus.Used)
            //{
            //    Panic.Error("01CA3000!!!");
            //}
            //KernelMessage.WriteLine("DEBUG-MARKER");
            //DumpPage(GetPhysPage(0x01CA3000));

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
                var fistPageNum = KMath.DivFloor(map.Start, 4096);
                KernelMessage.WriteLine("Mark Pages from {0:X8}, Size {1:X8}, Type {2}, FirstPage {3}, Pages {4}, Status {5}", map.Start, map.Size, (uint)map.Type, (uint)fistPageNum, mapPages, (uint)status);

                for (var p = fistPageNum; p < fistPageNum + mapPages; p++)
                {
                    var addr = p * 4096;
                    if (addr >= BootInfo.Header->InstalledPhysicalMemory)
                    {
                        KernelMessage.WriteLine("addr >= BootInfo.Header->InstalledPhysicalMemory");
                        break;
                    }
                    var page = GetPageByNum(p);
                    page->Status = status;
                }
            }
        }

        private void DumpPage(Page* p)
        {
            KernelMessage.WriteLine("pNum {0}, phys {1:X8} status {2} struct {3:X8} structPage {4}", p->PageNum, p->PhysicalAddress, (uint)p->Status, (uint)p, (uint)p / 4096);
        }

        public void Dump()
        {
            var sb = new StringBuffer();

            for (uint i = 0; i < PageCount; i++)
            {
                var p = &PageArray[i];
                if (i % 64 == 0)
                {
                    sb.Append("\nIndex={0} Page {1} at {2:X8}, PageStructAddr={3:X8}: ", i, p->PageNum, p->PhysicalAddress, (uint)p);
                    sb.WriteTo(DeviceManager.Serial1);
                    sb.Clear();
                }
                sb.Append((int)p->Status);
                sb.WriteTo(DeviceManager.Serial1);
                sb.Clear();
            }
        }

        public Page* GetPhysPage(Addr physAddr)
        {
            return GetPageByNum((uint)physAddr / PageSize);
        }

        public Page* GetPageByNum(uint pageNum)
        {
            if (pageNum > PageCount)
                return null;
            return &PageArray[pageNum];
        }

        //static uint _nextAllocacationSearchIndex;
        private static Page* lastAllocatedPage;

        /// <summary>
        /// Allocate a physical page from the free list
        /// </summary>
        /// <returns>The page</returns>
        private Page* Allocate(uint num)
        {
            lock (this)
            {
                if (num == 0)
                {
                    KernelMessage.WriteLine("Requesting zero pages");
                    return null;
                }
                else if (num > 1)
                {
                    KernelMessage.WriteLine("Requesting {0} pages", num);
                }

                //KernelMessage.WriteLine("Request {0} pages...", num);

                uint statBlocks = 0;
                uint statFreeBlocks = 0;
                int statMaxBlockPages = 0;
                uint statRangeChecks = 0;

                uint cnt = 0;

                if (lastAllocatedPage == null)
                    lastAllocatedPage = PageArray;

                Page* p = lastAllocatedPage->Next;
                while (true)
                {
                    statBlocks++;

                    if (p == null)
                        p = PageArray;

                    if (p->Status == PageStatus.Free)
                    {
                        statFreeBlocks++;
                        var head = p;

                        // Found free Page. Check now free range.
                        for (var i = 0; i < num; i++)
                        {
                            statRangeChecks++;
                            statMaxBlockPages = Math.Max(statMaxBlockPages, i);

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
                                    if (p->Status != PageStatus.Free)
                                        Panic.Error("Page is not Free. PageFrame Array corrupted?");

                                    p->Status = PageStatus.Used;
                                    p->Head = head;
                                    p->Tail = head->Tail;
                                    p = p->Next;
                                    FreePages--;
                                }
                                lastAllocatedPage = p;

                                //KernelMessage.WriteLine("Allocated from {0:X8} to {1:X8}", (uint)head->PhysicalAddress, (uint)head->Tail->PhysicalAddress + 4096 - 1);

                                //if (head->PhysicalAddress == 0x01CA4000)
                                //{
                                //    KernelMessage.WriteLine("DEBUG-MARKER 2");
                                //    DumpPage(head);
                                //}

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

                KernelMessage.WriteLine("Blocks={0} FreeBlocks={1} MaxBlockPages={2} RangeChecks={3} cnt={4}", statBlocks, statFreeBlocks, (uint)statMaxBlockPages, statRangeChecks, cnt);
                Dump();
                Panic.Error("PageFrameAllocator: Could not allocate " + num + " Pages.");
                return null;
            }
        }

        /// <summary>
        /// Releases a page to the free list
        /// </summary>
        private void Free(Addr address)
        {
            lock (this)
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
        }

        /// <summary>
        /// Retrieves the size of a single memory page.
        /// </summary>
        public static uint PageSize { get { return 4096; } }

        public uint PagesAvailable
        {
            get
            {
                return FreePages;
            }
        }

    }
}
