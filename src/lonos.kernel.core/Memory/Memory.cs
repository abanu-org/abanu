using System;
using Mosa.Kernel.x86;
using Mosa.Runtime.x86;

namespace lonos.kernel.core
{



    public static class Memory
    {

        private static Addr _startVirtAddr;
        private static Addr _nextVirtAddr;

        public static void Setup()
        {
            _startVirtAddr = 0x40000000; //1gb
            _nextVirtAddr = _startVirtAddr;

            uint reserveMem = 10 * 1024 * 1024;

            KernelMessage.WriteLine("Request {0} MB for MemoryAllocator", reserveMem / 1024 / 1024);

            _kmalloc_Next = RequestRawVirtalMemory(reserveMem);

            KernelMessage.WriteLine("Memory free: {0} MB", (PageFrameManager.PagesAvailable * 4096) / 1024 / 1024);
        }

        /// <summary>
        /// Returns raw, unmanaged Memory.
        /// Consumer: Kernel, Memory allocators
        /// Shoud be used for larger Chunks.
        /// </summary>
        public unsafe static Addr RequestRawVirtalMemoryPages(uint pages)
        {
            Addr virt = _nextVirtAddr;
            var head = PageFrameManager.AllocatePages(PageFrameRequestFlags.Default, pages);
            if (head == null)
                return Addr.Zero;

            var p = head;
            for (var i = 0; i < pages; i++)
            {
                PageTable.MapVirtualAddressToPhysical(_nextVirtAddr, p->PhysicalAddress);
                _nextVirtAddr += 4096;
                p = p->Next;
            }
            return virt;
        }

        public unsafe static void FreeRawVirtalMemoryPages(Addr virtAddr)
        {
        }

        private static uint RequiredPagesForSize(USize size)
        {
            return KMath.DivCeil(size, 4096);
        }

        private static void FreeRawVirtualMemory(uint size)
        {
            KernelMessage.WriteLine("NotImplemented"); ;
        }

        private static Addr _kmalloc_Next;

        /// <summary>
        /// kmalloc is the normal method of allocating memory for objects smaller than page size in the kernel.
        /// </summary>
        public static Addr Allocate(USize n, GFP flags)
        {
            var next = _kmalloc_Next;
            _kmalloc_Next += next;
            return next;
        }

        /// <summary>
        /// allocate memory. The memory is set to zero.
        /// </summary>
        public static Addr AllocateCleared(USize n, GFP flags)
        {
            var addr = Allocate(n, flags);
            MemoryOperation.Clear(addr, n);
            return addr;
        }

        /// <summary>
        /// allocate memory for an array.
        /// </summary>
        public static Addr AllocateArray(USize elements, USize size, GFP flags)
        {
            return Allocate(elements * size, flags);
        }

        /// <summary>
        /// allocate memory for an array. The memory is set to zero.
        /// </summary>
        public static Addr AllocateArrayCleared(USize elements, USize size, GFP flags)
        {
            var total = elements * size;
            var addr = Allocate(total, flags);
            MemoryOperation.Clear(addr, total);
            return addr;
        }

        /// <summary>
        /// 
        /// </summary>
        public static Addr AllocateVirtual(USize size)
        {
            return Addr.Zero;
        }

        /// <summary>
        /// free previously allocated memory
        /// </summary>
        public static void Free(Addr address)
        {
        }

        /// <summary>
        /// release memory allocated by vmalloc()
        /// </summary>
        public static void FreeVirtual(Addr address)
        {
        }

        public struct pgprot_t { }

        public unsafe static Addr MapVirtualPages(Page* pages, uint count, ulong flags, pgprot_t protection)
        {
            return Addr.Zero;
        }

    }

}
