// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using Lonos.Kernel.Core.PageManagement;

namespace Lonos.Kernel.Core.MemoryManagement
{
    public static class RawVirtualFrameAllocator
    {

        //private static Addr _startVirtAddr;
        //private static Addr _nextVirtAddr;

        private static VirtualPageAllocator Allocator;

        private static Addr _identityStartVirtAddr;
        private static Addr _identityNextVirtAddr;

        public static void Setup()
        {
            //_startVirtAddr = Address.VirtMapStart;
            //_nextVirtAddr = _startVirtAddr;

            var allocator = new VirtualPageAllocator();
            allocator.Initialize(new MemoryRegion(Address.VirtMapStart, 20 * 1024 * 1024));
            Allocator = allocator;

            _identityStartVirtAddr = Address.IdentityMapStart;
            _identityNextVirtAddr = _identityStartVirtAddr;

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
                return Addr.Zero;

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
                PageTable.KernelTable.MapVirtualAddressToPhysical(v->Address, p->Address);
                p = p->Next;
                v = v->Next;
            }
            PageTable.KernelTable.Flush();
            return virtHead->Address;
        }

        /// <summary>
        /// Returns pages, where virtAddr equals physAddr.
        /// </summary>
        internal static unsafe Addr RequestIdentityMappedVirtalMemoryPages(uint pages)
        {
            Addr virt = _identityNextVirtAddr;
            var head = PhysicalPageManager.GetPhysPage(virt);
            if (head == null)
                return Addr.Zero;

            var p = head;
            for (var i = 0; i < pages; i++)
            {
                p->Status = PageStatus.Used;
                PageTable.KernelTable.MapVirtualAddressToPhysical(_identityNextVirtAddr, p->Address);
                _identityNextVirtAddr += 4096;
                p = p->Next;
            }
            PageTable.KernelTable.Flush();
            return virt;
        }

        internal static unsafe void FreeRawVirtalMemoryPages(Addr virtAddr)
        {

        }

    }
}
