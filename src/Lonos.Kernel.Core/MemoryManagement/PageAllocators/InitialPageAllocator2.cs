// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;
using Lonos.CTypes;
using Lonos.Kernel.Core.Boot;
using Lonos.Kernel.Core.Devices;
using Lonos.Kernel.Core.Diagnostics;
using Lonos.Kernel.Core.PageManagement;
using Lonos.Kernel.Core.Scheduling;
using Mosa.Runtime;
using Mosa.Runtime.x86;

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

        public PageFrameAllocatorTraceOptions TraceOptions;

        protected abstract MemoryRegion AllocRawMemory(uint size);

        public void Setup(MemoryRegion region, AddressSpaceKind addrKind)
        {
            TraceOptions = new PageFrameAllocatorTraceOptions();
            _AddressSpaceKind = addrKind;
            _Region = region;
            FistPageNum = region.Start / PageSize;
            _TotalPages = region.Size / PageSize;
            kmap = AllocRawMemory(_TotalPages * (uint)sizeof(Page));
            PageArray = (Page*)kmap.Start;

            var firstSelfPageNum = KMath.DivFloor(kmap.Start, 4096);
            var selfPages = KMath.DivFloor(kmap.Size, 4096);

            KernelMessage.WriteLine("Page Frame Array allocated {0} pages, beginning with page {1} at {2:X8}", selfPages, firstSelfPageNum, (uint)PageArray);

            PageTable.KernelTable.SetWritable(kmap.Start, kmap.Size);
            kmap.Clear();

            var addr = FistPageNum * 4096;
            for (uint i = 0; i < _TotalPages; i++)
            {
                //KernelMessage.WriteLine(i);
                PageArray[i].Address = addr;
                //if (i != 0)
                //    PageArray[i - 1].Next = &PageArray[i];
                addr += 4096;
            }

            KernelMessage.WriteLine("Setup free memory");
            SetupFreeMemory();
            KernelMessage.WriteLine("Build linked lists");
            BuildLinkedLists();
            KernelMessage.WriteLine("Build linked lists done");

            _FreePages = 0;
            for (uint i = 0; i < _TotalPages; i++)
                if (PageArray[i].Status == PageStatus.Free)
                    _FreePages++;

            //Assert.True(list_head.list_count(FreeList) == _FreePages, "list_head.list_count(FreeList) == _FreePages");
            var debugCheckCount = list_head.list_count(FreeList);
            if (debugCheckCount != _FreePages)
            {
                KernelMessage.WriteLine("debugCheckCount {0} != {1}", debugCheckCount, _FreePages);
                Debug.Break();
            }

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        public Page* AllocatePage(AllocatePageOptions options = default)
        {
            return AllocatePages(1, options);
        }

        private list_head* FreeList;

        public Page* AllocatePages(uint pages, AllocatePageOptions options = default)
        {
            Page* page = AllocateInternal(pages, options);

            if (page == null)
            {
                //KernelMessage.WriteLine("DebugName: {0}", DebugName);
                KernelMessage.WriteLine("Free pages: {0:X8}, Requested: {1:X8}", FreePages, pages);
                Panic.Error("Out of Memory");
            }

            if (FreePages < 1000)
            {
                KernelMessage.Path(DebugName, "WARNING: Low pages. Available: {0}", FreePages);
            }

            return page;
        }

        private Page* AllocateInternal(uint pages, AllocatePageOptions options = default)
        {
            if (KConfig.Log.PageAllocation && TraceOptions.Enabled && pages >= TraceOptions.MinPages)
                KernelMessage.Path(DebugName, "Requesting Pages: {1}. Available: {2} DebugName={0}", options.DebugName, pages, _FreePages);

            if (pages == 256)
            {
                Debug.Nop();
            }

            UninterruptableMonitor.Enter(this);
            try
            {
                SelfCheck("SC1");
                if (pages > 1 && (AddressSpaceKind == AddressSpaceKind.Virtual || options.Continuous))
                {
                    if (!MoveToFreeContinuous(pages))
                    {
                        // Compact
                        //KernelMessage.Path(DebugName, "Compacting Linked List");
                        //this.DumpPages();
                        BuildLinkedLists();
                        if (!MoveToFreeContinuous(pages))
                        {
                            this.DumpPages();
                            KernelMessage.WriteLine("Requesting {0} pages failed", pages);
                            Panic.Error("Requesting pages failed: out of memory");
                        }
                    }
                }

                // ---
                var head = FreeList;
                var headPage = (Page*)head;
                FreeList = head->next;
                list_head.list_del_init(head);
                headPage->Status = PageStatus.Used;
                if (KConfig.Log.PageAllocation)
                    if (options.DebugName != null)
                        headPage->DebugTag = (uint)Intrinsic.GetObjectAddress(options.DebugName);
                    else
                        headPage->DebugTag = null;
                _FreePages--;
                // ---

                for (var i = 1; i < pages; i++)
                {
                    var tmpNextFree = FreeList->next;
                    list_head.list_move_tail(FreeList, head);
                    var p = (Page*)FreeList;
                    if (p->Status == PageStatus.Used)
                    {
                        this.DumpPages();
                        this.DumpPage(p);
                        KernelMessage.Path(DebugName, "Double Alloc pages={0} allocs={1} free={2} ptr={3:X8}", pages, (uint)_Requests, _FreePages, (uint)p);
                        Panic.Error("Double Alloc");
                    }
                    p->Status = PageStatus.Used;
                    FreeList = tmpNextFree;
                    _FreePages--;
                }

                if (KConfig.Log.PageAllocation && TraceOptions.Enabled && pages >= TraceOptions.MinPages)
                    KernelMessage.Path(DebugName, "Allocation done. Addr: {0:X8} Available: {1}", GetAddress(headPage), _FreePages);

                _Requests++;

                CheckAllocation(headPage, pages);
                SelfCheck("SC2");

                return headPage;
            }
            finally
            {
                UninterruptableMonitor.Exit(this);
            }
        }

        private void CheckAllocation(Page* page, uint pages)
        {
            //return;
            var count = list_head.list_count((list_head*)page);
            //Assert.True(count == pages);

            if (count != pages)
            {
                KernelMessage.Path(DebugName, "Pages {0} != {1}, num={2} addr={3:X8} ptr={4:X8}", pages, count, GetPageNum(page), GetAddress(page), (uint)page);
                Debug.Break();
            }
        }

        private void SelfCheck(string checkName, uint debugVal = 0)
        {
            return;
            var page = (Page*)FreeList;
            var count = list_head.list_count(FreeList);
            if (count != _FreePages)
            {
                this.DumpPages();
                this.DumpStats();
                this.DumpPage(page);
                KernelMessage.WriteLine("SelfCheck: {0} DebugVal={1}", checkName, debugVal);
                KernelMessage.Path(DebugName, "FreeListCount {0} != {1}, num={2} addr={3:X8} ptr={4:X8}", _FreePages, count, GetPageNum(page), GetAddress(page), (uint)page);
                Debug.Break();
            }
            else
            {
                //Serial.Write(Serial.COM1, (byte)'!');
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
            if (found)
            {
                FreeList = (list_head*)tryHead;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Releases a page to the free list
        /// </summary>
        public void Free(Page* page)
        {
            var oldFree = _FreePages;
            string debugName = null;
            if (page->DebugTag != null)
                debugName = (string)Intrinsic.GetObjectFromAddress((Pointer)(uint)page->DebugTag);

            UninterruptableMonitor.Enter(this);
            try
            {
                var debugCount = list_head.list_count((list_head*)page); // DEBUG

                SelfCheck("SCF1", debugCount);
                Page* temp = page;
                uint result = 0;

                do
                {
                    result++;
                    if (temp->Status == PageStatus.Free)
                    {
                        //Panic.Error("Double Free");
                        SelfCheck("SCF3", debugCount);
                        KernelMessage.WriteLine("Double Free. Pages {0} Iteration {1}", debugCount, result);
                        Debug.Break();
                    }

                    temp->Status = PageStatus.Free;

                    var oldTemp = temp;
                    temp = (Page*)temp->Lru.next;
                    Native.Nop();
                    _FreePages++;

                    list_head.list_move_tail((list_head*)oldTemp, FreeList);

                }
                while (temp != page && result != debugCount);

                //list_head.list_headless_splice_tail((list_head*)page, FreeList);
                SelfCheck("SCF2", debugCount);
            }
            finally
            {
                UninterruptableMonitor.Exit(this);
            }
            var freedPages = _FreePages - oldFree;
            if (KConfig.Log.PageAllocation && TraceOptions.Enabled && freedPages >= TraceOptions.MinPages)
                KernelMessage.Path(DebugName, "Freed Pages: {1}. Addr: {2:X8}. Now available: {3} --> {4}. Allocations={5} DebugName={0}.", debugName, freedPages, GetAddress(page), oldFree, _FreePages, (uint)Requests);

            _Releases++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetAddress(Page* page)
        {
            return page->Address;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetPageIndex(Page* page)
        {
            return GetPageNum(page) - FistPageNum;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetPageIndex(Addr addr)
        {
            return (addr / 4096) - FistPageNum;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        public uint MaxPagesPerAllocation => uint.MaxValue / 4096;
        public uint CriticalLowPages => 1000;

        public void SetTraceOptions(PageFrameAllocatorTraceOptions options)
        {
            TraceOptions = options;
        }

    }

}
