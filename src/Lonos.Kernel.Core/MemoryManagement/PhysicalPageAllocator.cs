// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using Lonos.Kernel.Core.Boot;
using Lonos.Kernel.Core.PageManagement;

namespace Lonos.Kernel.Core.MemoryManagement
{
    public unsafe class PhysicalPageAllocator : BasePageFrameAllocator
    {
        /// <summary>
        /// Setup the physical page manager
        /// </summary>
        public void Initialize()
        {
            uint physMem = BootInfo.Header->InstalledPhysicalMemory;
            var region = new MemoryRegion(Address.PhysMapStart, Address.PhysMapSize);

            Initialize(region);
        }

        protected override uint AllocRawMemory(uint size)
        {
            var kmap = KernelMemoryMapManager.Allocate(size, BootInfoMemoryType.PageFrameAllocator);
            PageTable.KernelTable.Map(kmap.Start, kmap.Start, kmap.Size, flush: true);
            PageTable.KernelTable.SetWritable(kmap.Start, kmap.Size);
            MemoryOperation.Clear4(kmap.Start, kmap.Size);
            return kmap.Start;
        }

    }
}
