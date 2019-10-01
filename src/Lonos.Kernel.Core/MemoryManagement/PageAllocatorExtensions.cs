// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using Lonos.Kernel.Core.Devices;

namespace Lonos.Kernel.Core.MemoryManagement
{

    public static unsafe class PageAllocatorExtensions
    {

        public static void DumpPage(this IPageFrameAllocator allocator, Page* p)
        {
            KernelMessage.WriteLine("pNum {0}, phys {1:X8} status {2} struct {3:X8} structPage {4}", allocator.GetPageNum(p), allocator.GetAddress(p), (uint)p->Status, (uint)p, (uint)p / 4096);
        }

        public static void Dump(this IPageFrameAllocator allocator)
        {
            var sb = new StringBuffer();

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
        }

        public static void FreeAddr(this IPageFrameAllocator allocator, Addr addr)
        {
            allocator.Free(allocator.GetPageByAddress(addr));
        }

        public static Addr AllocatePagesAddr(this IPageFrameAllocator allocator, uint pages, AllocatePageOptions options = AllocatePageOptions.Default)
        {
            return allocator.GetAddress(allocator.AllocatePages(pages, options));
        }

        public static Addr AllocatePageAddr(this IPageFrameAllocator allocator, AllocatePageOptions options = AllocatePageOptions.Default)
        {
            return allocator.GetAddress(allocator.AllocatePage(options));
        }

        public static MemoryRegion AllocateRegion(this IPageFrameAllocator allocator, USize size, AllocatePageOptions options = AllocatePageOptions.Default)
        {
            var pages = KMath.DivCeil(size, 4096);
            var p = allocator.AllocatePages(pages, options);
            return new MemoryRegion(allocator.GetAddress(p), pages * 4096);
        }

    }

}
