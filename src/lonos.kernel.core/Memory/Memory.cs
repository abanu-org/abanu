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

            _kmalloc_Next = RequestRawVirtalMemory(10 * 1024 * 1024);

            KernelMessage.WriteLine("Memory free: {0} MB", (PageFrameAllocator.PagesAvailable*4096)/1024/1024);
        }

        /// <summary>
        /// Returns raw, unmanaged Memory.
        /// Consumer: Kernel, Memory allocators
        /// Shoud be used for larger Chunks.
        /// </summary>
        private static Addr RequestRawVirtalMemory(USize size)
        {
            Addr virt = _nextVirtAddr;
            for (var i = 0; i < RequiredPagesForSize(size); i++)
            {
                var phys = PageFrameAllocator.Allocate();

                PageTable.MapVirtualAddressToPhysical(_nextVirtAddr, phys);
                _nextVirtAddr += 4096;
            }
            return virt;
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
            Clear(addr, n);
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
            Clear(addr, total);
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

        public unsafe static Addr MapVirtualPages(Page* pages, uint count, ulong flags, pgprot_t protection) {
            return Addr.Zero;
        }

        /// <summary>
        /// Clears the specified memory area.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="bytes">The bytes.</param>
        public static void Clear(Addr start, USize bytes)
        {
            if (bytes % 4 == 0)
            {
                Clear4(start, bytes);
                return;
            }

            for (uint at = start; at < (start + bytes); at++)
                Native.Set8(at, 0);
        }

        public static void Clear4(Addr start, USize bytes)
        {
            for (uint at = start; at < (start + bytes); at = at + 4)
                Native.Set32(at, 0);
        }

        public static void Copy(Addr source, Addr destination, USize length)
        {
            for (uint i = 0; i < length; i++)
                Native.Set8(destination + i, Native.Get8(source + i));  //TODO: Optimize with Set32
        }

    }

}
