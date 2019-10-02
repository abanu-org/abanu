// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
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

            var allocator = new VirtualInitialPageAllocator() { DebugName = "VirtInitial" };
            allocator.Setup(new MemoryRegion(Address.VirtMapStart, 60 * 1024 * 1024), AddressSpaceKind.Virtual);
            Allocator = allocator;

            //_identityStartVirtAddr = Address.IdentityMapStart;
            //_identityNextVirtAddr = _identityStartVirtAddr;

            allocator = new VirtualInitialPageAllocator() { DebugName = "VirtIdentityInitial" };
            allocator.Setup(new MemoryRegion(Address.IdentityMapStart, 60 * 1024 * 1024), AddressSpaceKind.Virtual);
            IdentityAllocator = allocator;
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

        internal static unsafe Addr AllocatePages(uint pages)
        {
            var physHead = PhysicalPageManager.AllocatePages(pages);
            if (physHead == null)
                return Addr.Zero;
            var virtHead = Allocator.AllocatePages(pages);

            var p = physHead;
            var v = virtHead;
            for (var i = 0; i < pages; i++)
            {
                PageTable.KernelTable.Map(Allocator.GetAddress(v), PhysicalPageManager.GetAddress(p));
                p = PhysicalPageManager.NextCompoundPage(p);
                v = Allocator.NextCompoundPage(v);
            }
            PageTable.KernelTable.Flush();
            return Allocator.GetAddress(virtHead);
        }

        /// <summary>
        /// Returns pages, where virtAddr equals physAddr.
        /// </summary>
        internal static unsafe Addr AllocateIdentityMappedPages(uint pages)
        {
            var virtHead = IdentityAllocator.AllocatePages(pages);

            var v = virtHead;
            for (var i = 0; i < pages; i++)
            {
                var addr = Allocator.GetAddress(v);
                PageTable.KernelTable.Map(addr, addr);
                v = Allocator.NextCompoundPage(v);
            }
            PageTable.KernelTable.Flush();
            return Allocator.GetAddress(virtHead);
        }

        internal static unsafe void FreeRawVirtalMemoryPages(Addr virtAddr)
        {

        }

    }
}
