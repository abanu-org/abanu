// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using Lonos.Kernel.Core.Boot;
using Lonos.Kernel.Core.Devices;
using Lonos.Kernel.Core.Diagnostics;
using Lonos.Kernel.Core.PageManagement;
using Mosa.Runtime;

namespace Lonos.Kernel.Core.MemoryManagement.PageAllocators
{

    /// <summary>
    /// This is a simple, but working Allocator. Its flexible to use.
    /// </summary>
    public abstract unsafe class InitialPageAllocator : IPageFrameAllocator
    {

        protected uint _FreePages;

        protected Page* PageArray;
        protected uint _TotalPages;

        private MemoryRegion kmap;

        private uint FistPageNum;

        protected abstract MemoryRegion AllocRawMemory(uint size);

        public void Setup(MemoryRegion region, AddressSpaceKind addrKind)
        {
            _Requests = 0;
            _Releases = 0;

            _AddressSpaceKind = addrKind;
            _Region = region;
            FistPageNum = region.Start / PageSize;
            _TotalPages = region.Size / PageSize;
            kmap = AllocRawMemory(_TotalPages * (uint)sizeof(Page));
            PageArray = (Page*)kmap.Start;
            NextTryPage = PageArray;

            var firstSelfPageNum = KMath.DivFloor(kmap.Start, 4096);
            var selfPages = KMath.DivFloor(kmap.Size, 4096);

            KernelMessage.WriteLine("Page Frame Array allocated {0} pages, beginning with page {1}", selfPages, firstSelfPageNum);

            PageTableExtensions.SetWritable(PageTable.KernelTable, kmap.Start, kmap.Size);
            MemoryOperation.Clear4(kmap.Start, kmap.Size);

            var addr = FistPageNum * 4096;
            for (uint i = 0; i < _TotalPages; i++)
            {
                PageArray[i].Address = addr;
                //if (i != 0)
                //    PageArray[i - 1].Next = &PageArray[i];
                addr += 4096;
            }

            SetupFreeMemory();

            _FreePages = 0;
            for (uint i = 0; i < _TotalPages; i++)
                if (PageArray[i].Status == PageStatus.Free)
                    _FreePages++;

            KernelMessage.WriteLine("Pages Free: {0}", FreePages);
        }

        protected abstract void SetupFreeMemory();

        public Page* GetPageByAddress(Addr physAddr)
        {
            return GetPageByNum(physAddr / PageSize);
        }

        public Page* GetPageByNum(uint pageNum)
        {
            var pageIdx = pageNum - FistPageNum;
            if (pageIdx > _TotalPages)
                return null;
            return &PageArray[pageIdx];
        }

        //static uint _nextAllocacationSearchIndex;
        private Page* NextTryPage;

        public Page* AllocatePage(AllocatePageOptions options = default)
        {
            return AllocatePages(1, options);
        }

        public Page* AllocatePages(uint pages, AllocatePageOptions options = default)
        {
            lock (this)
            {
                if (pages == 0)
                {
                    KernelMessage.WriteLine("Requesting zero pages");
                    return null;
                }
                else if (pages > 1 && KConfig.Log.PageAllocation)
                {
                    KernelMessage.WriteLine("Requesting {0} pages", pages);
                }

                //KernelMessage.WriteLine("Request {0} pages...", num);

                uint statBlocks = 0;
                uint statFreeBlocks = 0;
                int statMaxBlockPages = 0;
                uint statRangeChecks = 0;

                uint cnt = 0;

                if (NextTryPage == null)
                    NextTryPage = PageArray;

                Page* p = NextTryPage;
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
                        for (var i = 0; i < pages; i++)
                        {
                            statRangeChecks++;
                            statMaxBlockPages = Math.Max(statMaxBlockPages, i);

                            if (p == null)
                                break; // Reached end. Our Range is incomplete
                            if (p->Status != PageStatus.Free) // Used -> so we can abort the search
                                break;

                            if (i == pages - 1)
                            { // all loops successful. So we found our range.

                                if (p == null)
                                    Panic.Error("Tail is null");

                                head->Tail = p;
                                head->PagesUsed = pages;
                                p = head;
                                for (var n = 0; n < pages; n++)
                                {
                                    if (p->Status != PageStatus.Free)
                                        Panic.Error("Page is not Free. PageFrame Array corrupted?");

                                    p->Status = PageStatus.Used;
                                    p->Head = head;
                                    p->Tail = head->Tail;
                                    p = NextPage(p);
                                    _FreePages--;
                                }

                                // correct version:
                                NextTryPage = p;

                                // TODO: HACK! Currently, we have somewhere a buffer overrun? Fix that!
                                //NextTryPage = p + 1;

                                //var t = head->Tail;
                                //var a = t->Address;
                                //var anum = (uint)a;
                                ////(uint)head->Tail->Address + 4096 - 1

                                //KernelMessage.Write("<");
                                //KernelMessage.WriteLine("Allocated from {0:X8} to {1:X8}, Status={2}", (uint)head->Address, anum, (uint)head->Status);
                                //KernelMessage.Write(">");

                                //if (head->PhysicalAddress == 0x01CA4000)
                                //{
                                //    KernelMessage.WriteLine("DEBUG-MARKER 2");
                                //    DumpPage(head);
                                //}

                                return head;
                            }

                            p = NextPage(p);
                        }

                    }

                    if (p->Tail != null)
                        p = p->Tail;

                    p = NextPage(p);
                    if (++cnt > _TotalPages)
                        break;
                }

                KernelMessage.WriteLine("Blocks={0} FreeBlocks={1} MaxBlockPages={2} RangeChecks={3} cnt={4}", statBlocks, statFreeBlocks, (uint)statMaxBlockPages, statRangeChecks, cnt);
                this.DumpPages();
                Panic.Error("PageFrameAllocator: Could not allocate " + pages + " Pages.");
                return null;
            }
        }

        /// <summary>
        /// Releases a page to the free list
        /// </summary>
        public void Free(Page* page)
        {
            lock (this)
            {
                var head = page;
                if (head->Status == PageStatus.Reserved)
                    Panic.Error("Cannot free reserved page");

                //if (head->Free)
                if (head->Status == PageStatus.Free)
                {
                    Panic.Error("Double Free?");
                    return;
                }

                var num = head->PagesUsed;

                //KernelMessage.Write("F:{0};", num);

                var p = head;
                for (var n = 0; n < num; n++)
                {
                    if (p->Free)
                    {
                        Panic.Error("Already Free Page in Compound Page");
                        return;
                    }

                    p->Status = PageStatus.Free;
                    p->PagesUsed = 0;
                    p->Head = null;
                    p->Tail = null;
                    p = NextPage(p);
                    _FreePages++;
                }
                NextTryPage = head;
            }
        }

        public uint GetAddress(Page* page)
        {
            return page->Address;
        }

        public uint GetPageNum(Page* page)
        {
            return GetAddress(page) / 4096;
        }

        public Page* GetPageByIndex(uint pageIndex)
        {
            if (pageIndex >= _TotalPages)
                return null;
            return &PageArray[pageIndex];
        }

        public Page* NextPage(Page* page)
        {
            var pageIdx = GetPageIndex(page) + 1;
            if (pageIdx >= _TotalPages)
                return null;
            return &PageArray[pageIdx];
        }

        public Page* NextCompoundPage(Page* page)
        {
            if (page == null)
                return null;

            return NextPage(page);
        }

        public uint GetPageIndex(Page* page)
        {
            return GetPageNum(page) - FistPageNum;
        }

        public uint GetPageIndex(Addr addr)
        {
            return (addr / 4096) - FistPageNum;
        }

        public bool ContainsPage(Page* page)
        {
            return _Region.Contains(page->Address);
        }

        /// <summary>
        /// Gets the size of a single memory page.
        /// </summary>
        public static uint PageSize => 4096;

        public uint TotalPages => _TotalPages;

        private MemoryRegion _Region;
        public MemoryRegion Region => _Region;

        private AddressSpaceKind _AddressSpaceKind;
        public AddressSpaceKind AddressSpaceKind => _AddressSpaceKind;

        public uint FreePages => _FreePages;

        private ulong _Requests;
        public ulong Requests => _Requests;

        private ulong _Releases;
        public ulong Releases => _Releases;

        private string _DebugName;
        public string DebugName
        {
            get { return _DebugName; }
            set { _DebugName = value; }
        }

        public void SetTraceOptions(PageFrameAllocatorTraceOptions options)
        {
        }

    }

}
