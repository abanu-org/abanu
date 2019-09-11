// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lonos.Kernel.Core.Boot;
using Lonos.Kernel.Core.PageManagement;

namespace Lonos.Kernel.Core.MemoryManagement
{
    public static unsafe class PageTableExtensions
    {
        /// <summary>
        /// Sync specific mappings with another table.
        /// </summary>
        public static void MapCopy(this IPageTable table, IPageTable fromTable, BootInfoMemoryType type, bool present = true, bool flush = false)
        {
            var mm = BootInfo.GetMap(type);
            table.MapCopy(fromTable, mm->Start, mm->Size, present, flush);
        }

        public static unsafe void SetWritable(this IPageTable table, BootInfoMemoryType type)
        {
            var mm = BootInfo.GetMap(type);
            SetWritable(table, mm->Start, mm->Size);
        }

        public static unsafe void SetWritable(this IPageTable table, uint virtAddr, uint size)
        {
            if (!KConfig.UseKernelMemoryProtection)
                return;

            table.SetWritable(virtAddr, size);
        }

        public static unsafe void SetExecutable(this IPageTable table, BootInfoMemoryType type)
        {
            var mm = BootInfo.GetMap(type);
            SetExecutable(table, mm->Start, mm->Size);
        }

        public static unsafe void SetExecutable(this IPageTable table, uint virtAddr, uint size)
        {
            if (!KConfig.UseKernelMemoryProtection)
                return;

            table.SetExecutable(virtAddr, size);
        }

    }
}
