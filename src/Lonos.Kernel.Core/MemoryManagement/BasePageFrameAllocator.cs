// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using Lonos.Kernel.Core.Boot;
using Lonos.Kernel.Core.Devices;
using Lonos.Kernel.Core.Diagnostics;
using Lonos.Kernel.Core.PageManagement;
using Mosa.Runtime;

namespace Lonos.Kernel.Core.MemoryManagement
{

    /// <summary>
    /// A physical page allocator.
    /// </summary>
    public abstract unsafe class BasePageFrameAllocator : IPageFrameAllocator
    {

        private BuddyAllocatorImplementation.mem_zone Zone;
        private BuddyAllocatorImplementation.mem_zone* ZonePtr;
        protected Page* Pages;

        public BasePageFrameAllocator()
        {
        }

        internal virtual void Initialize(MemoryRegion region)
        {
            region.Size = KMath.FloorToPowerOfTwo(region.Size);
            _Region = region;
            var totalPages = region.Size >> BuddyAllocatorImplementation.BUDDY_PAGE_SHIFT;
            KernelMessage.WriteLine("Init Allocator: {0} Pages", totalPages);
            // init global memory block
            // all pages area
            var pages_size = totalPages * (uint)sizeof(Page);
            KernelMessage.WriteLine("Page Array Size in bytes: {0}", pages_size);
            Pages = (Page*)AllocRawMemory(pages_size);
            KernelMessage.WriteLine("Page Array Addr: {0:X8}", (uint)Pages);
            var start_addr = 0U;
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

        public Page* AllocatePages(uint pages)
        {
            return Allocate(pages);
        }

        public Page* AllocatePage()
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
            KernelMessage.WriteLine("pNum {0}, phys {1:X8} status {2} struct {3:X8} structPage {4}", GetPageNum(p), GetAddress(p), p->flags, (uint)p, (uint)p / 4096);
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
                sb.Append((int)p->flags);
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

    }
}
