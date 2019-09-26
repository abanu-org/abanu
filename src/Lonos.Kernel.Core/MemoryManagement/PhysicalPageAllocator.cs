// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using Lonos.Kernel.Core.Boot;
using Lonos.Kernel.Core.PageManagement;

namespace Lonos.Kernel.Core.MemoryManagement
{
    public unsafe class PhysicalPageAllocator : BasePageFrameAllocator
    {

        private KernelMemoryMap kmap;

        private MemoryRegion _Region;
        public override MemoryRegion Region => _Region;

        private uint _TotalPages;
        public override uint TotalPages => _TotalPages;

        private Page* PageArray;

        /// <summary>
        /// Setup the physical page manager
        /// </summary>
        public void Initialize()
        {
            uint physMem = BootInfo.Header->InstalledPhysicalMemory;
            _Region = new MemoryRegion(0, physMem);
            _TotalPages = physMem / PageSize;
            kmap = KernelMemoryMapManager.Allocate(TotalPages * (uint)sizeof(Page), BootInfoMemoryType.PageFrameAllocator);
            PageArray = (Page*)kmap.Start;

            var firstSelfPageNum = KMath.DivFloor(kmap.Start, 4096);
            var selfPages = KMath.DivFloor(kmap.Size, 4096);

            KernelMessage.WriteLine("Page Frame Array allocated {0} pages for its own, beginning with page {1}", selfPages, firstSelfPageNum);

            PageTableExtensions.SetWritable(PageTable.KernelTable, kmap.Start, kmap.Size);
            MemoryOperation.Clear4(kmap.Start, kmap.Size);

            for (uint i = 0; i < TotalPages; i++)
            {
                PageArray[i].Address = i * PageSize;
                if (i != 0)
                    PageArray[i - 1].Next = &PageArray[i];
            }

            SetupFreeMemory();
        }

        /// <summary>
        /// Setups the free memory.
        /// </summary>
        private unsafe void SetupFreeMemory()
        {
            if (!BootInfo.Present)
                return;

            for (uint i = 0; i < TotalPages; i++)
                PageArray[i].Status = PageStatus.Reserved;

            SetInitialPageStatus(&KernelMemoryMapManager.Header->SystemUsable, PageStatus.Free);
            SetInitialPageStatus(&KernelMemoryMapManager.Header->Used, PageStatus.Used);

            FreePages = 0;
            for (uint i = 0; i < TotalPages; i++)
                if (PageArray[i].Status == PageStatus.Free)
                    FreePages++;

            //if (GetPhysPage(0x01CA3000)->Status != PageStatus.Used)
            //{
            //    Panic.Error("01CA3000!!!");
            //}
            //KernelMessage.WriteLine("DEBUG-MARKER");
            //DumpPage(GetPhysPage(0x01CA3000));

            KernelMessage.WriteLine("Pages Free: {0}", FreePages);
        }

        private void SetInitialPageStatus(KernelMemoryMapArray* maps, PageStatus status)
        {
            for (var i = 0; i < maps->Count; i++)
            {
                var map = maps->Items[i];
                if (map.Start >= BootInfo.Header->InstalledPhysicalMemory)
                    continue;

                var mapPages = KMath.DivCeil(map.Size, 4096);
                var fistPageNum = KMath.DivFloor(map.Start, 4096);
                KernelMessage.WriteLine("Mark Pages from {0:X8}, Size {1:X8}, Type {2}, FirstPage {3}, Pages {4}, Status {5}", map.Start, map.Size, (uint)map.Type, (uint)fistPageNum, mapPages, (uint)status);

                for (var p = fistPageNum; p < fistPageNum + mapPages; p++)
                {
                    var addr = p * 4096;
                    if (addr >= BootInfo.Header->InstalledPhysicalMemory)
                    {
                        KernelMessage.WriteLine("addr >= BootInfo.Header->InstalledPhysicalMemory");
                        break;
                    }
                    var page = GetPageByNum(p);
                    page->Status = status;
                }
            }
        }

        public override Page* GetPageByNum(uint pageNum)
        {
            return GetPageByIndex(pageNum);
        }

        public override unsafe Page* GetPageByIndex(uint pageIndex)
        {
            if (pageIndex >= TotalPages)
                return null;
            return &PageArray[pageIndex];
        }
    }
}
