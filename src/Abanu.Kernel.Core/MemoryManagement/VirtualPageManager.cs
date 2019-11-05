// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System;
using Abanu.Kernel.Core.Diagnostics;
using Abanu.Kernel.Core.MemoryManagement.PageAllocators;
using Abanu.Kernel.Core.PageManagement;

namespace Abanu.Kernel.Core.MemoryManagement
{

    // VirtualPageManager depends on PhysicalPageManagaer

    /// <summary>
    /// Manages virtual Kernel Pages.
    /// </summary>
    public static class VirtualPageManager
    {

        private static IPageFrameAllocator NormalAllocator;
        private static IPageFrameAllocator IdentityAllocator;
        private static IPageFrameAllocator GlobalAllocator;

        public static void Setup()
        {
            NormalAllocator = CreateAllocatorStage1();
            NormalAllocator = CreateAllocatorStage2();

            var allocator2 = new VirtualInitialPageAllocator(false) { DebugName = "VirtIdentityInitial" };
            allocator2.Setup(new MemoryRegion(Address.IdentityMapStart, 60 * 1024 * 1024), AddressSpaceKind.Virtual);
            IdentityAllocator = allocator2;

            var allocator3 = new VirtualInitialPageAllocator(false) { DebugName = "GlobalInitial" };
            allocator3.Setup(new MemoryRegion(600 * 1024 * 1024, 100 * 1024 * 1024), AddressSpaceKind.Virtual);
            GlobalAllocator = allocator3;

            PhysicalPageManager.SelfTest();
            SelfTest(NormalAllocator);
            SelfTest(IdentityAllocator);
        }

        private static IPageFrameAllocator CreateAllocatorStage1()
        {
            var allocator = new VirtualInitialPageAllocator(true) { DebugName = "VirtInitial" };
            allocator.Setup(new MemoryRegion(Address.VirtMapStart, 32 * 1024 * 1024), AddressSpaceKind.Virtual);
            return allocator;

            //var allocator = new VirtualBuddyPageAllocator() { DebugName = "VirtBuddy" };
            //allocator.Setup(new MemoryRegion(Address.VirtMapStart, 32 * 1024 * 1024), AddressSpaceKind.Virtual);

            //var allocator2 = new VirtualInitialPageAllocator(false) { DebugName = "VirtInitial" };
            //allocator2.Setup(new MemoryRegion(Address.VirtMapStart + (32 * 1024 * 1024), 28 * 1024 * 1024), AddressSpaceKind.Virtual);

            //var multi = new MultiAllocator() { DebugName = "VirtMulti" };
            //multi.Initialize(new IPageFrameAllocator[] { allocator, allocator2 });
            //return multi;
        }

        public static IPageFrameAllocator CreateAllocatorStage2()
        {
            var allocator = new VirtualBuddyPageAllocator() { DebugName = "VirtBuddy" };
            allocator.Setup(new MemoryRegion(Address.VirtMapStart + (32 * 1024 * 1024), 32 * 1024 * 1024), AddressSpaceKind.Virtual);

            var multi = new MultiAllocator() { DebugName = "VirtMulti" };
            multi.Initialize(new IPageFrameAllocator[] { allocator, NormalAllocator });
            return multi;
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
            checkPageCount -= allocator.CriticalLowPages;
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

        private const bool AddProtectedRegions = false;

        public static unsafe Addr AllocatePages(uint pages, AllocatePageOptions options = default)
        {
            switch (options.Pool)
            {
                case PageAllocationPool.Normal:
                    return AllocatePagesNormal(pages, options);
                case PageAllocationPool.Identity:
                    return AllocateIdentityMappedPages(pages, options);
                case PageAllocationPool.Global:
                    return AllocateGlobalPages(pages, options);
                default:
                    Panic.Error("invalid pool");
                    break;
            }
            return Addr.Zero;
        }

        private static unsafe Addr AllocatePagesNormal(uint pages, AllocatePageOptions options = default)
        {
            if (AddProtectedRegions)
                pages += 2;

            var physHead = PhysicalPageManager.AllocatePages(pages, options);
            if (physHead == null)
                return Addr.Zero;
            var virtHead = NormalAllocator.AllocatePages(pages, options);

            var p = physHead;
            var v = virtHead;
            for (var i = 0; i < pages; i++)
            {
                var map = true;
                if (AddProtectedRegions && (i == 0 || i == pages - 1))
                    map = false;

                if (map)
                    PageTable.KernelTable.Map(NormalAllocator.GetAddress(v), PhysicalPageManager.GetAddress(p));

                p = PhysicalPageManager.NextCompoundPage(p);
                v = NormalAllocator.NextCompoundPage(v);
            }
            PageTable.KernelTable.Flush();

            if (AddProtectedRegions)
                virtHead = NormalAllocator.NextCompoundPage(virtHead);

            return NormalAllocator.GetAddress(virtHead);
        }

        /// <summary>
        /// Returns pages, where virtAddr equals physAddr.
        /// </summary>
        private static unsafe Addr AllocateIdentityMappedPages(uint pages, AllocatePageOptions options = default)
        {
            if (AddProtectedRegions)
                pages += 2;

            var virtHead = IdentityAllocator.AllocatePages(pages, options);

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

        private static unsafe Addr AllocateGlobalPages(uint pages, AllocatePageOptions options = default)
        {
            if (AddProtectedRegions)
                pages += 2;

            var physHead = PhysicalPageManager.AllocatePages(pages, options);
            if (physHead == null)
                return Addr.Zero;
            var virtHead = GlobalAllocator.AllocatePages(pages, options);

            var p = physHead;
            var v = virtHead;
            for (var i = 0; i < pages; i++)
            {
                var map = true;
                if (AddProtectedRegions && (i == 0 || i == pages - 1))
                    map = false;

                if (map)
                    PageTable.KernelTable.Map(GlobalAllocator.GetAddress(v), PhysicalPageManager.GetAddress(p));

                p = PhysicalPageManager.NextCompoundPage(p);
                v = GlobalAllocator.NextCompoundPage(v);
            }
            PageTable.KernelTable.Flush();

            if (AddProtectedRegions)
                virtHead = GlobalAllocator.NextCompoundPage(virtHead);

            return GlobalAllocator.GetAddress(virtHead);
        }

        internal static unsafe void FreeAddr(Addr addr)
        {
            if (IdentityAllocator.Region.Contains(addr))
            {
                FreeAddrIdentity(addr);
                return;
            }

            if (GlobalAllocator.Region.Contains(addr))
            {
                FreeAddrGlobal(addr);
                return;
            }

            FreeAddrNormal(addr);
        }

        private static unsafe void FreeAddrNormal(Addr addr)
        {
            var physAddr = PageTable.KernelTable.GetPhysicalAddressFromVirtual(addr);
            if (AddProtectedRegions)
                addr -= 4096;
            NormalAllocator.FreeAddr(addr);
            PhysicalPageManager.FreeAddr(physAddr);

            PageTable.KernelTable.UnMap(addr);
        }

        private static unsafe void FreeAddrIdentity(Addr addr)
        {
            if (AddProtectedRegions)
                addr -= 4096;
            IdentityAllocator.FreeAddr(addr);
            PageTable.KernelTable.UnMap(addr);
        }

        private static unsafe void FreeAddrGlobal(Addr addr)
        {
            var physAddr = PageTable.KernelTable.GetPhysicalAddressFromVirtual(addr);
            if (AddProtectedRegions)
                addr -= 4096;
            GlobalAllocator.FreeAddr(addr);
            PhysicalPageManager.FreeAddr(physAddr);

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
            NormalAllocator.SetTraceOptions(options);
            IdentityAllocator.SetTraceOptions(options);
        }

        public static void DumpStats()
        {
            NormalAllocator.DumpStats();
            IdentityAllocator.DumpStats();
            GlobalAllocator.DumpStats();
        }

    }
}
