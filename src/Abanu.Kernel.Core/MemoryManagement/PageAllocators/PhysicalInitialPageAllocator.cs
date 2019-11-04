// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System;
using Abanu.Kernel.Core.Boot;
using Abanu.Kernel.Core.Devices;
using Abanu.Kernel.Core.Diagnostics;
using Abanu.Kernel.Core.PageManagement;
using Mosa.Runtime;

//using Mosa.Kernel.x86;

namespace Abanu.Kernel.Core.MemoryManagement.PageAllocators
{

    public unsafe class PhysicalInitialPageAllocator : InitialPageAllocator2
    {

        protected override MemoryRegion AllocRawMemory(uint size)
        {
            var kmap = KernelMemoryMapManager.Allocate(size, BootInfoMemoryType.PageFrameAllocator, AddressSpaceKind.Both);
            KernelMessage.Path(DebugName, "AllocRawMemory: Done. Current maps:");
            KernelMemoryMapManager.PrintMapArrays();
            PageTable.KernelTable.Map(kmap.Start, kmap.Start, kmap.Size, flush: true);
            PageTable.KernelTable.SetWritable(kmap.Start, kmap.Size);
            var region = new MemoryRegion(kmap.Start, kmap.Size);
            region.Clear();
            return region;
        }

        /// <summary>
        /// Setups the free memory.
        /// </summary>
        protected override unsafe void SetupFreeMemory()
        {
            if (!BootInfo.Present)
                return;

            for (uint i = 0; i < _TotalPages; i++)
                PageArray[i].Status = PageStatus.Reserved;

            SetInitialPageStatus(&KernelMemoryMapManager.Header->SystemUsable, PageStatus.Free);
            SetInitialPageStatus(&KernelMemoryMapManager.Header->Used, PageStatus.Used);
            SetInitialPageStatus(&KernelMemoryMapManager.Header->KernelReserved, PageStatus.Used);
        }

        private void SetInitialPageStatus(KernelMemoryMapArray* maps, PageStatus status)
        {
            for (var i = 0; i < maps->Count; i++)
            {
                var map = maps->Items[i];
                if (map.Start >= BootInfo.Header->InstalledPhysicalMemory)
                    continue;

                if ((map.AddressSpaceKind & AddressSpaceKind.Physical) == 0)
                    continue;

                var mapPages = KMath.DivCeil(map.Size, 4096);
                var fistPageNum = KMath.DivFloor(map.Start, 4096);
                KernelMessage.WriteLine("Mark Pages from {0:X8}, Size {1:X8}, Type {2}, FirstPage {3}, Pages {4}, Status {5}", map.Start, map.Size, (uint)map.Type, (uint)fistPageNum, mapPages, (uint)status);

                for (var p = fistPageNum; p < fistPageNum + mapPages; p++)
                {
                    var addr = p * 4096;
                    if (!Region.Contains(addr))
                        continue;

                    if (addr >= BootInfo.Header->InstalledPhysicalMemory)
                    {
                        KernelMessage.WriteLine("addr >= BootInfo.Header->InstalledPhysicalMemory");
                        break;
                    }
                    var page = GetPageByNum(p);
                    Assert.IsSet(page, "page == null");
                    page->Status = status;
                }
            }
        }

    }

}
