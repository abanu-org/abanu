// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using Lonos.Kernel.Core.MemoryManagement.PageAllocators;
using Lonos.Kernel.Core.PageManagement;

namespace Lonos.Kernel.Core.MemoryManagement
{
    public static class VirtualPageManager
    {

        private static IPageFrameAllocator Allocator;
        private static IPageFrameAllocator IdentityAllocator;

        //private static Addr _startVirtAddr;
        //private static Addr _nextVirtAddr;

        //private static Addr _identityStartVirtAddr;
        //private static Addr _identityNextVirtAddr;

        public static void Setup()
        {
            //_startVirtAddr = Address.VirtMapStart;
            //_nextVirtAddr = _startVirtAddr;

            var allocator = new VirtualInitialPageAllocator(true) { DebugName = "VirtInitial" };
            //allocator.Setup(MemoryRegion.FromLocation(0, Address.VirtMapStart + (60 * 1024 * 1024)), AddressSpaceKind.Virtual);
            allocator.Setup(new MemoryRegion(Address.VirtMapStart, 60 * 1024 * 1024), AddressSpaceKind.Virtual);
            Allocator = allocator;

            //_identityStartVirtAddr = Address.IdentityMapStart;
            //_identityNextVirtAddr = _identityStartVirtAddr;

            allocator = new VirtualInitialPageAllocator(false) { DebugName = "VirtIdentityInitial" };
            allocator.Setup(new MemoryRegion(Address.IdentityMapStart, 60 * 1024 * 1024), AddressSpaceKind.Virtual);
            IdentityAllocator = allocator;
        }

        private static void UnmapFreePages()
        {
            Addr addr = 0;
            while (addr < Address.MaximumMemory)
            {
                addr += 4096;
            }
        }

        //internal static unsafe Addr AllocatePages(uint pages)
        //{
        //    Addr virt = _nextVirtAddr;
        //    var head = PhysicalPageManager.AllocatePages(pages);
        //    if (head == null)
        //        return Addr.Zero;

        //    var p = head;
        //    for (var i = 0; i < pages; i++)
        //    {
        //        PageTable.KernelTable.MapVirtualAddressToPhysical(_nextVirtAddr, PhysicalPageManager.GetAddress(p));
        //        _nextVirtAddr += 4096;
        //        p = PhysicalPageManager.NextCompoundPage(p);
        //    }
        //    PageTable.KernelTable.Flush();
        //    return virt;
        //}

        private const bool AddProtectedRegions = true;

        internal static unsafe Addr AllocatePages(uint pages, AllocatePageOptions options = default)
        {
            if (AddProtectedRegions)
                pages += 2;

            var physHead = PhysicalPageManager.AllocatePages(pages, options);
            if (physHead == null)
                return Addr.Zero;
            var virtHead = Allocator.AllocatePages(pages, options);

            var p = physHead;
            var v = virtHead;
            for (var i = 0; i < pages; i++)
            {
                var map = true;
                if (AddProtectedRegions && (i == 0 || i == pages - 1))
                    map = false;

                if (map)
                    PageTable.KernelTable.Map(Allocator.GetAddress(v), PhysicalPageManager.GetAddress(p));

                p = PhysicalPageManager.NextCompoundPage(p);
                v = Allocator.NextCompoundPage(v);
            }
            PageTable.KernelTable.Flush();

            if (AddProtectedRegions)
                virtHead = Allocator.NextCompoundPage(virtHead);

            return Allocator.GetAddress(virtHead);
        }

        /// <summary>
        /// Returns pages, where virtAddr equals physAddr.
        /// </summary>
        internal static unsafe Addr AllocateIdentityMappedPages(uint pages)
        {
            if (AddProtectedRegions)
                pages += 2;

            var virtHead = IdentityAllocator.AllocatePages(pages);

            var v = virtHead;
            for (var i = 0; i < pages; i++)
            {
                var addr = Allocator.GetAddress(v);

                var map = true;
                if (AddProtectedRegions && (i == 0 || i == pages - 1))
                    map = false;

                if (map)
                    PageTable.KernelTable.Map(addr, addr);
                v = Allocator.NextCompoundPage(v);
            }
            PageTable.KernelTable.Flush();

            if (AddProtectedRegions)
                virtHead = Allocator.NextCompoundPage(virtHead);

            return Allocator.GetAddress(virtHead);
        }

        internal static unsafe void FreeAddr(Addr addr)
        {
            var physAddr = PageTable.KernelTable.GetPhysicalAddressFromVirtual(addr);
            if (AddProtectedRegions)
                addr -= 4096;
            Allocator.FreeAddr(addr);
            PhysicalPageManager.FreeAddr(physAddr);
        }

        internal static unsafe void FreeAddrIdentity(Addr addr)
        {
            if (AddProtectedRegions)
                addr -= 4096;
            IdentityAllocator.FreeAddr(addr);
        }

        public static MemoryRegion AllocateRegion(USize size, AllocatePageOptions options = default)
        {
            size = KMath.DivCeil(size, 4096);
            var start = AllocatePages(size, options);
            return new MemoryRegion(start, size);
        }

        public static void SetTraceOptions(PageFrameAllocatorTraceOptions options)
        {
            Allocator.SetTraceOptions(options);
            IdentityAllocator.SetTraceOptions(options);
        }

    }
}
