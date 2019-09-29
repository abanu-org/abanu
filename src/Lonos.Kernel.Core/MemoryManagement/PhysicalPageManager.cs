// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using Lonos.Kernel.Core.Boot;
using Lonos.Kernel.Core.MemoryManagement.PageAllocators;
using Lonos.Kernel.Core.PageManagement;

namespace Lonos.Kernel.Core.MemoryManagement
{
    public static unsafe class PhysicalPageManager
    {

        private static IPageFrameAllocator Default;
        public const uint PageSize = 4096;

        public static void Setup()
        {
            KernelMessage.WriteLine("Initialize PhysicalPageManager");

            uint physMem = BootInfo.Header->InstalledPhysicalMemory;
            var pageArraySize = (physMem / 4096) * (uint)sizeof(Page);
            KernelMessage.WriteLine("Requesting Size {0:X8} for pageArray", pageArraySize);
            var allPagesMap = KernelMemoryMapManager.Allocate(pageArraySize, BootInfoMemoryType.PageFrameAllocator);
            PageTable.KernelTable.Map(allPagesMap.Start, allPagesMap.Start, allPagesMap.Size, flush: true);
            PageTable.KernelTable.SetWritable(allPagesMap.Start, allPagesMap.Size);
            MemoryOperation.Clear4(allPagesMap.Start, allPagesMap.Size);
            var allPages = (Page*)allPagesMap.Start;

            var phys = new SimplePageAllocator() { DebugName = "SimplePhys" };
            phys.Initialize(new MemoryRegion(Address.PhysMapStart, Address.PhysMapSize), &allPages[Address.PhysMapStart / 4096], AddressSpaceKind.Physical);

            var simpl = new SimplePageAllocator() { DebugName = "SimplePhys2" };
            simpl.Initialize(new MemoryRegion(Address.PhysMapStart2, Address.PhysMapSize2), &allPages[Address.PhysMapStart2 / 4096], AddressSpaceKind.Physical);

            var multi = new MultiAllocator();
            multi.Initialize(new IPageFrameAllocator[] { phys, simpl });

            Default = multi;
        }

        public static Page* AllocatePages(uint pages, AllocatePageOptions options = AllocatePageOptions.Default)
        {
            return Default.AllocatePages(pages, options);
        }

        public static Page* AllocatePage(AllocatePageOptions options = AllocatePageOptions.Default)
        {
            var p = Default.AllocatePage(options);
            //if (p->PhysicalAddress == 0x01CA4000)
            //    Panic.Error("DEBUG-MARKER");
            return p;
        }

        public static Addr AllocatePagesAddr(uint pages, AllocatePageOptions options = AllocatePageOptions.Default)
        {
            return GetAddress(AllocatePages(pages, options));
        }

        public static Addr AllocatePageAddr(AllocatePageOptions options = AllocatePageOptions.Default)
        {
            return GetAddress(AllocatePage(options));
        }

        public static MemoryRegion AllocateRegion(USize size, AllocatePageOptions options = AllocatePageOptions.Default)
        {
            var pages = KMath.DivCeil(size, 4096);
            var p = AllocatePages(pages, options);
            return new MemoryRegion(Default.GetAddress(p), pages * 4096);
        }

        public static void Free(Page* page)
        {
            Default.Free(page);
        }

        public static Page* GetPhysPage(Addr physAddr)
        {
            return Default.GetPageByAddress(physAddr);
        }

        public static Page* GetPageByNum(uint pageNum)
        {
            return Default.GetPageByNum(pageNum);
        }

        public static Page* GetPageByIndex(uint pageIndex)
        {
            return Default.GetPageByIndex(pageIndex);
        }

        public static uint PagesAvailable
        {
            get
            {
                return Default.FreePages;
            }
        }

        public static uint GetAddress(Page* page)
        {
            return Default.GetAddress(page);
        }

        public static Page* NextPage(Page* page)
        {
            return page + 1;
        }

        public static Page* NextCompoundPage(Page* page)
        {
            return Default.NextCompoundPage(page);
        }

    }
}
