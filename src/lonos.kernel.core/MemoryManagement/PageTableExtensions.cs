using lonos.Kernel.Core.Boot;
using lonos.Kernel.Core.PageManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lonos.Kernel.Core.MemoryManagement
{
    public unsafe static class PageTableExtensions
    {
        /// <summary>
        /// Sync specific mappings with another table.
        /// </summary>
        public static void MapCopy(this IPageTable table, IPageTable fromTable, BootInfoMemoryType type, bool present = true, bool flush = false)
        {
            var mm = BootInfo.GetMap(type);
            table.MapCopy(fromTable, mm->Start, mm->Size, present, flush);
        }

        public unsafe static void WritableByMapType(this IPageTable table, BootInfoMemoryType type)
        {
            var mm = BootInfo.GetMap(type);
            WritableBySize(table, mm->Start, mm->Size);
        }

        public unsafe static void WritableBySize(this IPageTable table, uint virtAddr, uint size)
        {
            if (!KConfig.UseKernelMemoryProtection)
                return;

            table.SetKernelWriteProtectionForRegion(virtAddr, size);
        }

        public unsafe static void ExecutableByMapType(this IPageTable table, BootInfoMemoryType type)
        {
            var mm = BootInfo.GetMap(type);
            ExecutableBySize(table, mm->Start, mm->Size);
        }

        public unsafe static void ExecutableBySize(this IPageTable table, uint virtAddr, uint size)
        {
            if (!KConfig.UseKernelMemoryProtection)
                return;

            table.SetExecutableForRegion(virtAddr, size);
        }


    }
}
