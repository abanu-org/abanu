// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using Abanu.Kernel.Core.Devices;
using Abanu.Kernel.Core.MemoryManagement.PageAllocators;

namespace Abanu.Kernel.Core.MemoryManagement
{

    /// <summary>
    /// Extension methods for the <see cref="IPageFrameAllocator"/> interface
    /// </summary>
    public static unsafe class PageAllocatorExtensions
    {

        public static void DumpPage(this IPageFrameAllocator allocator, Page* p)
        {
            KernelMessage.WriteLine("pNum {0}, phys {1:X8} status {2} struct {3:X8} structPage {4}", allocator.GetPageNum(p), allocator.GetAddress(p), (uint)p->Status, (uint)p, (uint)p / 4096);
        }

        public static void DumpPages(this IPageFrameAllocator allocator)
        {
            var sb = new StringBuffer();

            sb.Append("Allocator Dump of {0}. TotalPages={1} Free={2}", allocator.DebugName, allocator.TotalPages, allocator.FreePages);

            for (uint i = 0; i < allocator.TotalPages; i++)
            {
                var p = allocator.GetPageByIndex(i);
                if (i % 64 == 0)
                {
                    sb.Append("\nIndex={0} Page {1} at {2:X8}, PageStructAddr={3:X8}: ", i, allocator.GetPageNum(p), allocator.GetAddress(p), (uint)p);
                    sb.WriteTo(DeviceManager.Serial1);
                    sb.Clear();
                }
                sb.Append((int)p->Status);
                sb.WriteTo(DeviceManager.Serial1);
                sb.Clear();
            }
            DeviceManager.Serial1.Write('\n');
        }

        /// <summary>
        /// Dump statistics of interest.
        /// </summary>
        public static void DumpStats(this IPageFrameAllocator allocator)
        {
            KernelMessage.WriteLine("Stats for {0}", allocator.DebugName);
            KernelMessage.WriteLine("TotalPages {0}, FreePages {1}, Requests {2}, Releases {3}, Allocations {4}", allocator.TotalPages, allocator.FreePages, (uint)allocator.Requests, (uint)allocator.Releases, (uint)(allocator.Requests - allocator.Releases));
            if (allocator is MultiAllocator)
            {
                var multi = (MultiAllocator)allocator;
                for (var i = 0; i < multi.Allocators.Length; i++)
                    multi.Allocators[i].DumpStats();
            }
        }

        public static void FreeAddr(this IPageFrameAllocator allocator, Addr addr)
        {
            allocator.Free(allocator.GetPageByAddress(addr));
        }

        public static Addr AllocatePagesAddr(this IPageFrameAllocator allocator, uint pages, AllocatePageOptions options = default)
        {
            return allocator.GetAddress(allocator.AllocatePages(pages, options));
        }

        public static Addr AllocatePageAddr(this IPageFrameAllocator allocator, AllocatePageOptions options = default)
        {
            return allocator.GetAddress(allocator.AllocatePage(options));
        }

        public static MemoryRegion AllocateRegion(this IPageFrameAllocator allocator, USize size, AllocatePageOptions options = default)
        {
            var pages = KMath.DivCeil(size, 4096);
            var p = allocator.AllocatePages(pages, options);
            return new MemoryRegion(allocator.GetAddress(p), pages * 4096);
        }

    }

}
