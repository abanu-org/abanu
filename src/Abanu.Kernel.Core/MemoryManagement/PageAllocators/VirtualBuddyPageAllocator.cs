// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using Abanu.Kernel.Core.Boot;
using Abanu.Kernel.Core.PageManagement;

namespace Abanu.Kernel.Core.MemoryManagement.PageAllocators
{
    public unsafe class VirtualBuddyPageAllocator : BuddyPageAllocator
    {

        protected override MemoryRegion AllocRawMemory(uint size)
        {
            var kmap = PhysicalPageManager.AllocateRegion(size);
            KernelMemoryMapManager.Header->Used.Add(new KernelMemoryMap(kmap.Start, kmap.Size, BootInfoMemoryType.PageFrameAllocator, AddressSpaceKind.Virtual));
            PageTable.KernelTable.Map(kmap.Start, kmap.Start, PhysicalPageManager.GetAllocatorByAddr(kmap.Start), flush: true);
            PageTable.KernelTable.SetWritable(kmap.Start, kmap.Size);
            kmap.Clear();
            return kmap;
        }

    }
}
