// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using Lonos.Kernel.Core.Boot;
using Lonos.Kernel.Core.Diagnostics;
using Lonos.Kernel.Core.PageManagement;

namespace Lonos.Kernel.Core.MemoryManagement
{
    public unsafe class VirtualPageAllocator : BasePageFrameAllocator
    {

        private MemoryRegion kmap;

        private uint firstPageNum;

        private uint _totalPages;
        public override uint TotalPages => _totalPages;

        private MemoryRegion _Region;
        public override MemoryRegion Region => _Region;

        private Page* PageArray;

        /// <summary>
        /// Setup the physical page manager
        /// </summary>
        public void Initialize(MemoryRegion virtRegion)
        {
            Panic.Error("tet");
            _Region = virtRegion;
            firstPageNum = KMath.DivFloor(virtRegion.Start, 4096);
            _totalPages = KMath.DivCeil(virtRegion.Size, 4096);
            kmap = PhysicalPageManager.AllocateRegion(TotalPages * (uint)sizeof(Page));
            PageArray = (Page*)kmap.Start;

            var firstSelfPageNum = KMath.DivFloor(kmap.Start, 4096);
            var selfPages = KMath.DivFloor(kmap.Size, 4096);

            KernelMessage.WriteLine("Virtual Page Frame Array allocated {0} pages, beginning with page {1}", selfPages, firstSelfPageNum);

            PageTableExtensions.SetWritable(PageTable.KernelTable, kmap.Start, kmap.Size);
            MemoryOperation.Clear4(kmap.Start, kmap.Size);

            FreePages = TotalPages;
            for (uint i = 0; i < TotalPages; i++)
            {
                PageArray[i].Address = virtRegion.Start + (i * PageSize);
                PageArray[i].Status = PageStatus.Free;
                if (i != 0)
                    PageArray[i - 1].Next = &PageArray[i];
            }

            KernelMessage.WriteLine("Virtual Kernel Pages Free: {0}", FreePages);
        }

        public override Page* GetPageByNum(uint pageNum)
        {
            return GetPageByIndex(pageNum - firstPageNum);
        }

        public override unsafe Page* GetPageByIndex(uint pageIndex)
        {
            if (pageIndex > TotalPages)
                return null;
            return &PageArray[pageIndex];
        }
    }
}
