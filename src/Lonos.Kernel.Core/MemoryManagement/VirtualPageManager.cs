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

            //lockObj = new object();
            //LockCount = 0;

            Allocator = CreateAllocator();

            //_identityStartVirtAddr = Address.IdentityMapStart;
            //_identityNextVirtAddr = _identityStartVirtAddr;

            var allocator2 = new VirtualInitialPageAllocator(false) { DebugName = "VirtIdentityInitial" };
            allocator2.Setup(new MemoryRegion(Address.IdentityMapStart, 60 * 1024 * 1024), AddressSpaceKind.Virtual);
            IdentityAllocator = allocator2;

            PhysicalPageManager.SelfTest();
            SelfTest(Allocator);
            SelfTest(IdentityAllocator);
        }

        private static IPageFrameAllocator CreateAllocator()
        {
            var allocator = new VirtualInitialPageAllocator(true) { DebugName = "VirtInitial" };
            allocator.Setup(new MemoryRegion(Address.VirtMapStart, 60 * 1024 * 1024), AddressSpaceKind.Virtual);
            return allocator;

            //var allocator = new VirtualBuddyPageAllocator() { DebugName = "VirtBuddy" };
            //allocator.Setup(new MemoryRegion(Address.VirtMapStart, 32 * 1024 * 1024), AddressSpaceKind.Virtual);
            //return allocator;

            //var allocator2 = new VirtualInitialPageAllocator(false) { DebugName = "VirtInitial" };
            //allocator2.Setup(new MemoryRegion(Address.VirtMapStart + (32 * 1024 * 1024), 28 * 1024 * 1024), AddressSpaceKind.Virtual);

            //var multi = new MultiAllocator();
            //multi.Initialize(new IPageFrameAllocator[] { allocator, allocator2 });
            //return multi;
        }

        private const bool SelfTestDump = false;

        public static unsafe void SelfTest(IPageFrameAllocator allocator)
        {
            if (SelfTestDump)
                allocator.DumpPages();

            KernelMessage.WriteLine("Begin SelfTest {0}", allocator.DebugName);

            var ptrPages = (allocator.TotalPages * 4) / 4096;
            var ptrListAddr = AllocatePages(ptrPages); // pointers for 4GB of pages
            var ptrList = (Addr*)ptrListAddr;
            var checkPageCount = allocator.FreePages;
            checkPageCount -= 1000;
            uint checkPagesEach = 4;
            checkPageCount /= checkPagesEach;
            //checkPageCount = 32;
            var mapPhysAddr = PhysicalPageManager.AllocatePageAddr(checkPagesEach);
            for (var i = 0; i < checkPageCount; i++)
            {
                if (SelfTestDump)
                    KernelMessage.Write(".");
                var testAddr = allocator.AllocatePagesAddr(checkPagesEach);
                ptrList[i] = testAddr;
                //KernelMessage.WriteLine("{0:X8}-->{1:X8}", testAddr, mapPhysAddr);
                PageTable.KernelTable.Map(testAddr, mapPhysAddr, 4096 * checkPagesEach, true, true);
                var mapPtr = (uint*)testAddr;
                for (var pos = 0; pos < 1024 * checkPagesEach; pos++)
                {
                    *mapPtr = 0xEBFEEBFE;
                    mapPtr += 1;
                }
                PageTable.KernelTable.UnMap(testAddr, 4096 * checkPagesEach, true);
                //Default.Free(testPage);
            }
            PhysicalPageManager.FreeAddr(mapPhysAddr);

            if (SelfTestDump)
                allocator.DumpPages();

            KernelMessage.WriteLine("Free Pages now");
            for (var i = 0; i < checkPageCount; i++)
            {
                if (SelfTestDump)
                    KernelMessage.Write(":");
                var testAddr = ptrList[i];
                //KernelMessage.WriteLine("Free: {0:X8}", testAddr);

                allocator.FreeAddr(testAddr);
            }
            KernelMessage.WriteLine("Free ptrList");
            FreeAddr(ptrListAddr);

            KernelMessage.WriteLine("SelfTest Done");
            if (SelfTestDump)
            {
                allocator.DumpPages();
                KernelMessage.WriteLine("Final Dump");
            }
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

        private const bool AddProtectedRegions = false;

        //private static object lockObj;
        //public static int LockCount = 0;

        internal static unsafe Addr AllocatePages(uint pages, AllocatePageOptions options = default)
        {
            if (AddProtectedRegions)
                pages += 2;

            //lock (lockObj)
            //{
            //    LockCount++;
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

            //LockCount--;
            return Allocator.GetAddress(virtHead);
            //}
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
                var addr = IdentityAllocator.GetAddress(v);

                var map = true;
                if (AddProtectedRegions && (i == 0 || i == pages - 1))
                    map = false;

                if (map)
                    PageTable.KernelTable.Map(addr, addr);
                v = IdentityAllocator.NextCompoundPage(v);
            }
            PageTable.KernelTable.Flush();

            if (AddProtectedRegions)
                virtHead = IdentityAllocator.NextCompoundPage(virtHead);

            return IdentityAllocator.GetAddress(virtHead);
        }

        internal static unsafe void FreeAddr(Addr addr)
        {
            var physAddr = PageTable.KernelTable.GetPhysicalAddressFromVirtual(addr);
            if (AddProtectedRegions)
                addr -= 4096;
            Allocator.FreeAddr(addr);
            PhysicalPageManager.FreeAddr(physAddr);

            PageTable.KernelTable.UnMap(addr);
        }

        internal static unsafe void FreeAddrIdentity(Addr addr)
        {
            if (AddProtectedRegions)
                addr -= 4096;
            IdentityAllocator.FreeAddr(addr);
            PageTable.KernelTable.UnMap(addr);
        }

        public static MemoryRegion AllocateRegion(USize size, AllocatePageOptions options = default)
        {
            var pages = KMath.DivCeil(size, 4096);
            var start = AllocatePages(pages, options);
            return new MemoryRegion(start, pages * 4096);
        }

        public static void SetTraceOptions(PageFrameAllocatorTraceOptions options)
        {
            Allocator.SetTraceOptions(options);
            IdentityAllocator.SetTraceOptions(options);
        }

        public static void DumpStats()
        {
            Allocator.DumpStats();
            IdentityAllocator.DumpStats();
        }

    }
}
