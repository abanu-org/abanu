// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using Lonos.CTypes;
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
    public abstract unsafe class InitialPageAllocator2 : IPageFrameAllocator
    {

        protected uint _FreePages;

        protected Page* PageArray;
        protected uint _TotalPages;

        private MemoryRegion kmap;

        private uint FistPageNum;

        public string DebugName;

        protected abstract MemoryRegion AllocRawMemory(uint size);

        public void Setup(MemoryRegion region, AddressSpaceKind addrKind)
        {
            _AddressSpaceKind = addrKind;
            _Region = region;
            FistPageNum = region.Start / PageSize;
            _TotalPages = region.Size / PageSize;
            kmap = AllocRawMemory(_TotalPages * (uint)sizeof(Page));
            PageArray = (Page*)kmap.Start;

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

            KernelMessage.WriteLine("Setup free memory");
            SetupFreeMemory();
            KernelMessage.WriteLine("Build linked lists");
            BuildLinkedLists();

            _FreePages = 0;
            for (uint i = 0; i < _TotalPages; i++)
                if (PageArray[i].Status == PageStatus.Free)
                    _FreePages++;

            Assert.True(list_head.list_count(FreeList) == _FreePages, "list_head.list_count(FreeList) == _FreePages");

            KernelMessage.Path(DebugName, "Pages Free: {0}", FreePages);
        }

        protected void BuildLinkedLists()
        {
            FreeList = null;
            for (var i = 0; i < _TotalPages; i++)
            {
                var p = &PageArray[i];
                if (p->Free)
                {
                    if (FreeList == null)
                    {
                        FreeList = (list_head*)p;
                        list_head.INIT_LIST_HEAD(FreeList);
                    }
                    else
                    {
                        list_head.list_add_tail((list_head*)p, FreeList);
                    }
                }
            }
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

        public Page* AllocatePage(AllocatePageOptions options = AllocatePageOptions.Default)
        {
            return AllocatePages(1, options);
        }

        private list_head* FreeList;

        public Page* AllocatePages(uint pages, AllocatePageOptions options = AllocatePageOptions.Default)
        {
            Page* page = AllocateInternal(pages, options);

            if (page == null)
            {
                //KernelMessage.WriteLine("DebugName: {0}", DebugName);
                KernelMessage.WriteLine("Free pages: {0:X8}, Requested: {1:X8}, Options {2}", FreePages, pages, (uint)options);
                Panic.Error("Out of Memory");
            }

            return page;
        }

        private Page* AllocateInternal(uint pages, AllocatePageOptions options = AllocatePageOptions.Default)
        {
            if (pages > 1)
                KernelMessage.Path(DebugName, "Requesting Pages: {0}. Available: {1}", pages, _FreePages);

            lock (this)
            {
                if (pages > 1 && (AddressSpaceKind == AddressSpaceKind.Virtual || (options & AllocatePageOptions.Continuous) == AllocatePageOptions.Continuous))
                {
                    if (!MoveToFreeContinuous(pages))
                    {
                        // Compact
                        KernelMessage.WriteLine("Compacting Linked List");
                        BuildLinkedLists();
                        if (!MoveToFreeContinuous(pages))
                        {
                            this.Dump();
                            Panic.Error("out of memory");
                        }
                    }
                }

                // ---
                var head = FreeList;
                FreeList = head->next;
                list_head.list_del_init(head);
                ((Page*)head)->Status = PageStatus.Used;
                _FreePages--;
                // ---

                for (var i = 1; i < pages; i++)
                {
                    var tmpNextFree = FreeList->next;
                    list_head.list_move_tail(FreeList, head);
                    ((Page*)FreeList)->Status = PageStatus.Used;
                    FreeList = tmpNextFree;
                    _FreePages--;
                }

                if (pages > 1)
                    KernelMessage.Path(DebugName, "Allocation done. Addr: {0:X8} Available: {1}", GetAddress((Page*)head), _FreePages);

                return (Page*)head;
            }

        }

        private bool MoveToFreeContinuous(uint pages)
        {
            Page* tryHead = (Page*)FreeList;
            var loopedPages = 0;

            Page* tmpHead = tryHead;
            var found = false;
            for (int i = 0; i < pages; i++)
            {
                if (loopedPages >= _FreePages)
                    return false;
                loopedPages++;

                var next = (Page*)tmpHead->Lru.next;
                if (GetPageNum(next) - GetPageNum(tmpHead) != 1)
                {
                    tryHead = next;
                    tmpHead = next;
                    i = -1; // Reset loop
                    continue;
                }

                tmpHead = next;

                if (i == pages - 1)
                    found = true;
            }
            FreeList = (list_head*)tryHead;
            return true;
        }

        /// <summary>
        /// Releases a page to the free list
        /// </summary>
        public void Free(Page* page)
        {
            var oldFree = _FreePages;
            lock (this)
            {
                Page* temp = page;
                uint result = 0;

                do
                {
                    temp = (Page*)temp->Lru.next;
                    _FreePages++;
                    temp->Status = PageStatus.Free;
                }
                while (temp != page);

                list_head.list_headless_splice_tail((list_head*)page, FreeList);
            }
            var freedPages = _FreePages - oldFree;
            if (freedPages > 1)
                KernelMessage.Path(DebugName, "Freed Pages: {0}. Addr: {1:X8}. Now available: {2} --> {3}", freedPages, GetAddress(page), oldFree, _FreePages);
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

            return (Page*)page->Lru.next;
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

    }

}
