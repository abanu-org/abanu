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
    /// A physical page allocator.
    /// </summary>
    public abstract unsafe class BuddyPageAllocator : IPageFrameAllocator
    {

        private BuddyAllocatorImplementation.mem_zone Zone;
        private BuddyAllocatorImplementation.mem_zone* ZonePtr;
        protected Page* Pages;

        public BuddyPageAllocator()
        {
        }

        internal virtual void Initialize(MemoryRegion region, Page* pages, AddressSpaceKind addressSpaceKind)
        {
            AddressSpaceKind = addressSpaceKind;
            region.Size = KMath.FloorToPowerOfTwo(region.Size);
            _Region = region;
            var totalPages = region.Size >> BuddyAllocatorImplementation.BUDDY_PAGE_SHIFT;
            KernelMessage.WriteLine("Init Allocator: StartAddr: {0}, {1} Pages", region.Start, totalPages);
            // init global memory block
            // all pages area
            var pages_size = totalPages * (uint)sizeof(Page);
            KernelMessage.WriteLine("Page Array Size in bytes: {0}", pages_size);
            Pages = pages;
            KernelMessage.WriteLine("Page Array Addr: {0:X8}", (uint)Pages);
            var start_addr = region.Start;
            Zone.free_area = (BuddyAllocatorImplementation.free_area*)AllocRawMemory(BuddyAllocatorImplementation.BUDDY_MAX_ORDER * (uint)sizeof(BuddyAllocatorImplementation.free_area));

            fixed (BuddyAllocatorImplementation.mem_zone* zone = &Zone)
                ZonePtr = zone;

            BuddyAllocatorImplementation.buddy_system_init(
                ZonePtr,
                Pages,
                start_addr,
                totalPages);
        }

        protected abstract uint AllocRawMemory(uint size);

        public Page* AllocatePages(uint pages, AllocatePageOptions options = default)
        {
            return Allocate(pages);
        }

        public Page* AllocatePage(AllocatePageOptions options = default)
        {
            var p = Allocate(1);
            return p;
        }

        private Page* Allocate(uint num)
        {
            return BuddyAllocatorImplementation.buddy_get_pages(ZonePtr, GetOrderForPageCount(num));
        }

        private static byte GetOrderForPageCount(uint pages)
        {
            return (byte)KMath.Log2OfPowerOf2(KMath.CeilToPowerOfTwo(pages));
        }

        public uint FreePages => BuddyAllocatorImplementation.buddy_num_free_page(ZonePtr);

        public uint TotalPages => Zone.page_num;

        private void DumpPage(Page* p)
        {
            KernelMessage.WriteLine("pNum {0}, phys {1:X8} status {2} struct {3:X8} structPage {4}", GetPageNum(p), GetAddress(p), p->Flags, (uint)p, (uint)p / 4096);
        }

        public void Dump()
        {
            var sb = new StringBuffer();

            for (uint i = 0; i < TotalPages; i++)
            {
                var p = GetPageByIndex(i);
                if (i % 64 == 0)
                {
                    sb.Append("\nIndex={0} Page {1} at {2:X8}, PageStructAddr={3:X8}: ", i, GetPageNum(p), GetAddress(p), (uint)p);
                    sb.WriteTo(DeviceManager.Serial1);
                    sb.Clear();
                }
                sb.Append((int)p->Flags);
                sb.WriteTo(DeviceManager.Serial1);
                sb.Clear();
            }
        }

        public Page* GetPageByAddress(Addr addr)
        {
            return BuddyAllocatorImplementation.virt_to_page(ZonePtr, addr);
        }

        public Page* GetPageByNum(uint pageNum)
        {
            return BuddyAllocatorImplementation.virt_to_page(ZonePtr, (void*)(pageNum << BuddyAllocatorImplementation.BUDDY_PAGE_SHIFT));
        }

        public Page* GetPageByIndex(uint pageIndex)
        {
            if (pageIndex < 0 || pageIndex >= TotalPages)
                return null;
            return &Pages[pageIndex];
        }

        public void Free(Page* page)
        {
            BuddyAllocatorImplementation.buddy_free_pages(ZonePtr, page);
        }

        public uint GetPageNum(Page* page)
        {
            return (uint)BuddyAllocatorImplementation.page_to_virt(ZonePtr, page) >> BuddyAllocatorImplementation.BUDDY_PAGE_SHIFT;
        }

        public uint GetAddress(Page* page)
        {
            return (uint)BuddyAllocatorImplementation.page_to_virt(ZonePtr, page);
        }

        public Page* NextPage(Page* page)
        {
            return page + 1;
        }

        public uint GetPageIndex(Page* page)
        {
            var addr = GetAddress(page);
            return (addr - Region.Start) / 4096;
        }

        public uint GetPageIndex(Addr addr)
        {
            return (addr - Region.Start) / 4096;
        }

        public bool ContainsPage(Page* page)
        {
            var addr = GetAddress(page);
            return Region.Contains(addr);
        }

        /// <summary>
        /// Gets the size of a single memory page.
        /// </summary>
        public static uint PageSize => 4096;

        private MemoryRegion _Region;
        public MemoryRegion Region
        {
            get
            {
                return _Region;
            }
        }

        public AddressSpaceKind AddressSpaceKind { get; private set; }

        public Page* NextCompoundPage(Page* page)
        {
            var next = NextPage(page);
            if (next == null || next->FirstPage != page->FirstPage)
                return null;

            return next;
        }

        public void SetTraceOptions(PageFrameAllocatorTraceOptions options)
        {
        }
    }
}
