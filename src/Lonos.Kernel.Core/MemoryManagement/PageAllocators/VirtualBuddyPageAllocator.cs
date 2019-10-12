// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using Lonos.Kernel.Core.Boot;
using Lonos.Kernel.Core.PageManagement;

namespace Lonos.Kernel.Core.MemoryManagement.PageAllocators
{
    public unsafe class VirtualBuddyPageAllocator : BuddyPageAllocator
    {

        protected override MemoryRegion AllocRawMemory(uint size)
        {
            var kmap = PhysicalPageManager.AllocateRegion(size);
            KernelMemoryMapManager.Header->Used.Add(new KernelMemoryMap(kmap.Start, kmap.Size, BootInfoMemoryType.PageFrameAllocator, AddressSpaceKind.Virtual));
            PageTable.KernelTable.Map(kmap.Start, kmap.Start, kmap.Size, flush: true);
            PageTable.KernelTable.SetWritable(kmap.Start, kmap.Size);
            MemoryOperation.Clear4(kmap.Start, kmap.Size);
            return new MemoryRegion(kmap.Start, kmap.Size);
        }

    }
}
