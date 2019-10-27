// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using Abanu.Kernel.Core.Boot;
using Abanu.Kernel.Core.Collections;
using Abanu.Kernel.Core.PageManagement;

namespace Abanu.Kernel.Core.MemoryManagement.PageAllocators.PageAllocators
{
    public unsafe class PhysicalBuddyPageAllocator : BuddyPageAllocator
    {

        protected override MemoryRegion AllocRawMemory(uint size)
        {
            var kmap = KernelMemoryMapManager.Allocate(size, BootInfoMemoryType.PageFrameAllocator, AddressSpaceKind.Both);
            PageTable.KernelTable.Map(kmap.Start, kmap.Start, kmap.Size, flush: true);
            PageTable.KernelTable.SetWritable(kmap.Start, kmap.Size);
            var region = new MemoryRegion(kmap.Start, kmap.Size);
            region.Clear();
            return region;
        }

    }

}
