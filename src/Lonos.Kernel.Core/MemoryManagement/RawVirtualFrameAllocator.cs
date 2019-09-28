// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using Lonos.Kernel.Core.Diagnostics;
using Lonos.Kernel.Core.PageManagement;

namespace Lonos.Kernel.Core.MemoryManagement
{
    public static class RawVirtualFrameAllocator
    {

        //private static Addr _startVirtAddr;
        //private static Addr _nextVirtAddr;

        private static IPageFrameAllocator Allocator;
        private static IPageFrameAllocator IdentityAllocator;

        public static void Setup()
        {
            //_startVirtAddr = Address.VirtMapStart;
            //_nextVirtAddr = _startVirtAddr;

            KernelMessage.WriteLine("Initialize VirualPageAllocator");

            var allocator = new VirtualPageAllocator();
            allocator.Initialize(new MemoryRegion(Address.VirtMapStart, 100 * 1024 * 1024));
            Allocator = allocator;

            KernelMessage.WriteLine("Initialize VirualIdentityPageAllocator");

            var identityAllocator = new VirtualPageAllocator();
            identityAllocator.Initialize(new MemoryRegion(Address.IdentityMapStart, 20 * 1024 * 1024));
            IdentityAllocator = identityAllocator;
        }

        /// <summary>
        /// Returns raw, unmanaged Memory.
        /// Consumer: Kernel, Memory allocators
        /// Shoud be used for larger Chunks.
        /// </summary>
        internal static unsafe Addr RequestRawVirtalMemoryPages(uint pages)
        {
            var physHead = PhysicalPageManager.AllocatePages(pages);
            if (physHead == null)
            {
                return Addr.Zero;
            }

            var virtHead = Allocator.AllocatePages(pages);
            if (virtHead == null)
            {
                PhysicalPageManager.Free(physHead);
                return Addr.Zero;
            }

            var p = physHead;
            var v = virtHead;
            for (var i = 0; i < pages; i++)
            {
                PageTable.KernelTable.MapVirtualAddressToPhysical(Allocator.GetAddress(v), PhysicalPageManager.GetAddress(p));
                p = PhysicalPageManager.NextPage(p);
                v = Allocator.NextPage(v);
            }
            PageTable.KernelTable.Flush();

            KernelMessage.WriteLine("VirtAllocator: Allocated {0} Pages at {1:X8}, PageNum {2}, Remaining {3} Pages", pages, Allocator.GetAddress(virtHead), Allocator.GetPageNum(virtHead), Allocator.FreePages);

            return Allocator.GetAddress(virtHead);
        }

        /// <summary>
        /// Returns pages, where virtAddr equals physAddr.
        /// </summary>
        internal static unsafe Addr RequestIdentityMappedVirtalMemoryPages(uint pages)
        {
            // TODO: Ensure region is reserved in virtual address space

            var p = PhysicalPageManager.AllocatePages(pages);
            if (p == null)
            {
                return Addr.Zero;
            }

            for (var i = 0; i < pages; i++)
            {
                PageTable.KernelTable.MapVirtualAddressToPhysical(PhysicalPageManager.GetAddress(p), PhysicalPageManager.GetAddress(p));
                p = PhysicalPageManager.NextPage(p);
            }
            PageTable.KernelTable.Flush();
            return PhysicalPageManager.GetAddress(p);
        }

        internal static unsafe void FreeRawVirtalMemoryPages(Addr virtAddr)
        {
            var p = Allocator.GetPageByAddress(virtAddr);
            if (p == null)
            {
                KernelMessage.WriteLine("Virtual Allocator: Free(): Invalid Page Addr {0:X8}", virtAddr);
                return;
            }

            Allocator.Free(p);

            KernelMessage.WriteLine("Virtual Allocator: Free PageNum {0} at {1:X8}, Remaining Pages: {2}", Allocator.GetPageNum(p), virtAddr, Allocator.FreePages);
        }

        public static uint PagesAvailable
        {
            get
            {
                return Allocator.FreePages;
            }
        }

    }
}
