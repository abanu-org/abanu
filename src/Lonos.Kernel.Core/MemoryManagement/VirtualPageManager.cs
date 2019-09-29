// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using Lonos.Kernel.Core.Boot;
using Lonos.Kernel.Core.Diagnostics;
using Lonos.Kernel.Core.MemoryManagement.PageAllocators;
using Lonos.Kernel.Core.PageManagement;

namespace Lonos.Kernel.Core.MemoryManagement
{

    public static unsafe class VirtualPageManager
    {

        //private static Addr _startVirtAddr;
        //private static Addr _nextVirtAddr;

        private static IPageFrameAllocator Allocator;
        private static IPageFrameAllocator IdentityAllocator;

        private static MemoryRegion AllocSelf(uint size)
        {
            // TODO: Do not use AllocatePageOptions.Continuous.

            size = KMath.AlignValueCeil(size, 4096);
            var allPagesMapPageHead = PhysicalPageManager.AllocatePages(KMath.DivCeil(size, 4096), AllocatePageOptions.Continuous);

            var next = allPagesMapPageHead;
            var headAddr = PhysicalPageManager.GetAddress(allPagesMapPageHead);
            while (next != null)
            {
                var pageAddr = PhysicalPageManager.GetAddress(next);
                PageTable.KernelTable.Map(pageAddr, pageAddr, 4096, flush: true);
                PageTable.KernelTable.SetWritable(pageAddr, 4096);
                next = PhysicalPageManager.NextCompoundPage(next);
            }
            MemoryOperation.Clear4(headAddr, size);
            return new MemoryRegion(headAddr, size);
        }

        public static void Setup()
        {
            KernelMessage.WriteLine("Initialize VirtualPageManager");

            //_startVirtAddr = Address.VirtMapStart;
            //_nextVirtAddr = _startVirtAddr;

            KernelMessage.WriteLine("Initialize VirtualPageAllocator");
            var pageArraySize = 0x100000 * (uint)sizeof(Page);
            KernelMessage.WriteLine("Requesting Size {0:X8} for pageArray", pageArraySize);

            var allPages = (Page*)AllocSelf(pageArraySize).Start;

            var allocator = new SimplePageAllocator() { DebugName = "SimpleVirt" };
            allocator.Initialize(new MemoryRegion(Address.VirtMapStart, 100 * 1024 * 1024), &allPages[Address.VirtMapStart / 4096], AddressSpaceKind.Virtual);
            Allocator = allocator;

            KernelMessage.WriteLine("Initialize VirtualIdentityPageAllocator");

            var identityAllocator = new SimplePageAllocator() { DebugName = "SimpleVirtIdent" };
            identityAllocator.Initialize(new MemoryRegion(Address.IdentityMapStart, 20 * 1024 * 1024), &allPages[Address.IdentityMapStart / 4096], AddressSpaceKind.Virtual);
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
