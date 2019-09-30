// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using Lonos.Kernel.Core.Boot;
using Lonos.Kernel.Core.PageManagement;

namespace Lonos.Kernel.Core.MemoryManagement
{
    public static unsafe class PhysicalPageManager
    {

        private static PageFrameAllocator Default;
        public const uint PageSize = 4096;

        public static void Setup()
        {
            Default = new PageFrameAllocator();
            Default.Setup(new MemoryRegion(0, BootInfo.Header->InstalledPhysicalMemory), AddressSpaceKind.Physical);

            //SelfTest();
        }

        public static void SelfTest()
        {
            Default.Dump();

            KernelMessage.WriteLine("Begin SelfTest");

            var ptrMem = (Addr*)AllocatePageAddr(2);
            var checkPageCount = Default.FreePages;
            checkPageCount -= 1000;
            //checkPageCount = 32;
            var mapAddr = 0x2000u;
            for (var i = 0; i < checkPageCount; i++)
            {
                KernelMessage.Write(".");
                var testPage = Default.AllocatePage();
                var testAddr = testPage->Address;
                Default.Free(testPage);
                PageTable.KernelTable.Map(mapAddr, testAddr, 4096, true, true);
                PageTable.KernelTable.UnMap(mapAddr, 4096, true);
            }
            KernelMessage.WriteLine("SelfTest Done");
            Default.Dump();
            KernelMessage.WriteLine("Final Dump");
        }

        public static Page* AllocatePages(uint pages, AllocatePageOptions options = AllocatePageOptions.Default)
        {
            return Default.AllocatePages(pages);
        }

        public static Page* AllocatePage(AllocatePageOptions options = AllocatePageOptions.Default)
        {
            var p = Default.AllocatePage();
            //if (p->PhysicalAddress == 0x01CA4000)
            //    Panic.Error("DEBUG-MARKER");
            return p;
        }

        public static Addr AllocatePageAddr(uint pages, AllocatePageOptions options = AllocatePageOptions.Default)
        {
            return AllocatePages(pages)->Address;
        }

        public static Addr AllocatePageAddr(AllocatePageOptions options = AllocatePageOptions.Default)
        {
            return AllocatePage()->Address;
        }

        public static MemoryRegion AllocateRegion(USize size, AllocatePageOptions options = AllocatePageOptions.Default)
        {
            var pages = KMath.DivCeil(size, 4096);
            var p = AllocatePages(pages, options);
            return new MemoryRegion(p->Address, pages * 4096);
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

        public static uint PagesAvailable
        {
            get
            {
                return Default.PagesAvailable;
            }
        }

    }
}
