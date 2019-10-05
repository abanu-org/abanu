// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using Lonos.Kernel.Core.Boot;
using Mosa.Runtime.x86;

namespace Lonos.Kernel.Core.PageManagement
{
    public static class PageTableExtensions
    {
        public static void Map(this IPageTable table, Addr virtAddr, Addr physAddr, USize length, bool present = true, bool flush = false)
        {
            if (KConfig.Trace.MemoryMapping && length > 4096)
                KernelMessage.WriteLine("Map: virt={0:X8}, phys={1:X8}, length={2:X8}", virtAddr, physAddr, length);

            var pages = KMath.DivCeil(length, 4096);
            for (var i = 0; i < pages; i++)
            {
                table.MapVirtualAddressToPhysical(virtAddr, physAddr, present);

                virtAddr += 4096;
                physAddr += 4096;
            }
            if (flush)
                table.Flush();
        }

        public static void Map(this IPageTable table, Addr virtAddr, Addr physAddr, bool present = true, bool flush = false)
        {
            Map(table, virtAddr, physAddr, 4096, present, flush);
        }

        public static void UnMap(this IPageTable table, Addr virtAddr, bool flush = false)
        {
            UnMap(table, virtAddr, 4096, flush);
        }

        public static void UnMap(this IPageTable table, Addr virtAddr, USize length, bool flush = false)
        {
            if (KConfig.Trace.MemoryMapping && length > 4096)
                KernelMessage.WriteLine("UnMap: virt={0:X8}, length={2:X8}", virtAddr, length);

            var pages = KMath.DivCeil(length, 4096);
            for (var i = 0; i < pages; i++)
            {
                table.MapVirtualAddressToPhysical(virtAddr, 0, false);

                virtAddr += 4096;
            }
            if (flush)
                table.Flush();
        }

        /// <summary>
        /// Maps two tables at the same time, with same virt and phys address
        /// </summary>
        public static void MapDual(this IPageTable table, IPageTable sharedTable, Addr virtAddr, Addr physAddr, USize length, bool present = true, bool flush = false)
        {
            if (KConfig.Trace.MemoryMapping && length > 4096)
                KernelMessage.WriteLine("MapDual: virt={0:X8}, phys={1:X8}, length={2:X8}", virtAddr, physAddr, length);

            var pages = KMath.DivCeil(length, 4096);
            for (var i = 0; i < pages; i++)
            {
                table.MapVirtualAddressToPhysical(virtAddr, physAddr, present);
                sharedTable.MapVirtualAddressToPhysical(virtAddr, physAddr, present);

                virtAddr += 4096;
                physAddr += 4096;
            }
            if (flush)
                table.Flush();
        }

        /// <summary>
        /// Sync specific mappings with another table.
        /// </summary>
        public static void MapCopy(this IPageTable table, IPageTable fromTable, Addr virtAddr, USize length, bool present = true, bool flush = false)
        {
            if (KConfig.Trace.MemoryMapping && length > 4096)
                KernelMessage.WriteLine("MapCopy: virt={0:X8}, length={1:X8}", virtAddr, length);

            var pages = KMath.DivCeil(length, 4096);
            for (var i = 0; i < pages; i++)
            {
                var physAddr = fromTable.GetPhysicalAddressFromVirtual(virtAddr);
                table.MapVirtualAddressToPhysical(virtAddr, physAddr, present);

                virtAddr += 4096;
            }
            if (flush)
                table.Flush();
        }

        /// <summary>
        /// Sync specific mappings with another table.
        /// </summary>
        public static void MapCopy(this IPageTable table, IPageTable fromTable, Addr srcVirtAddr, Addr destVirtAddr, USize length, bool present = true, bool flush = false)
        {
            if (KConfig.Trace.MemoryMapping && length > 4096)
                KernelMessage.WriteLine("MapCopy: srcVirt={0:X8}, destVirt={1:X8}, length={2:X8}", srcVirtAddr, destVirtAddr, length);

            var pages = KMath.DivCeil(length, 4096);
            for (var i = 0; i < pages; i++)
            {
                var physAddr = fromTable.GetPhysicalAddressFromVirtual(srcVirtAddr);
                table.MapVirtualAddressToPhysical(destVirtAddr, physAddr, present);

                srcVirtAddr += 4096;
                destVirtAddr += 4096;
            }
            if (flush)
                table.Flush();
        }

        public static Addr GetPageTablePhysAddr(this IPageTable table)
        {
            return PageTable.KernelTable.GetPhysicalAddressFromVirtual(table.VirtAddr);
        }
    }

}
