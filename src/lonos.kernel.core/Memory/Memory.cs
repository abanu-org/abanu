using System;
using Mosa.Kernel.x86;
using Mosa.Runtime.x86;

namespace lonos.kernel.core
{



    public static class Memory
    {

        public unsafe static void Setup()
        {
            kmallocAllocator = new Allocator();

            var ptr = (byte*)RawVirtualFrameAllocator.RequestRawVirtalMemoryPages(KMath.AlignValueCeil(Allocator.headSize, 4096));
            for (var i = 0; i < Allocator.headSize; i++)
                *(ptr + i) = 0;
            kmallocAllocator.list_heads = (malloc_meta**)ptr;
            ManagedMemoy.useAllocator = true;

            KernelMessage.WriteLine("Memory free: {0} MB", (PageFrameManager.PagesAvailable * 4096) / 1024 / 1024);
        }


        private static uint RequiredPagesForSize(USize size)
        {
            return KMath.DivCeil(size, 4096);
        }

        private static void FreeRawVirtualMemory(uint size)
        {
            KernelMessage.WriteLine("NotImplemented"); ;
        }

        private static Allocator kmallocAllocator;

        /// <summary>
        /// kmalloc is the normal method of allocating memory for objects smaller than page size in the kernel.
        /// </summary>
        public unsafe static Addr Allocate(USize n, GFP flags)
        {
            return kmallocAllocator.malloc(n);
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
        public unsafe static void Free(Addr address)
        {
            kmallocAllocator.free(address);
        }

        /// <summary>
        /// free previously allocated memory
        /// </summary>
        public unsafe static void Free(IntPtr address)
        {
            kmallocAllocator.free((void*)address);
        }

        public unsafe static void FreeObject(object obj)
        {
            var ptr = Mosa.Runtime.Intrinsic.GetObjectAddress(obj);
            kmallocAllocator.free((void*)ptr);
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
