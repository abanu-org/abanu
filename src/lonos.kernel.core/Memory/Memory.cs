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

        // COMPILER BUG: The presence of the following Methods causes a "Page not mapped" error.

        public static bool UseKernelWriteProtection;

        public unsafe static void InitialKernelProtect()
        {
            if (!UseKernelWriteProtection)
                return;

            //KernelMessage.WriteLine("Unset CR0.WP");
            Native.SetCR0((uint)(Native.GetCR0() & ~0x10000));

            PageTable.PageTableEntry* pte = (PageTable.PageTableEntry*)PageTable.AddrPageTable;
            for (int index = 0; index < 1024 * 32; index++)
            {
                var e = &pte[index];
                e->Writable = false;
            }

            InitialKernelProtect_MakeWritable_ByMapType(BootInfoMemoryType.PageDirectory);
            InitialKernelProtect_MakeWritable_ByMapType(BootInfoMemoryType.PageTable);
            InitialKernelProtect_MakeWritable_ByMapType(BootInfoMemoryType.InitialStack);

            //KernelMessage.WriteLine("Reload CR3 to {0:X8}", PageTable.AddrPageDirectory);
            Native.SetCR3(PageTable.AddrPageDirectory);

            //KernelMessage.WriteLine("Set CR0.WP");
            Native.SetCR0((uint)(Native.GetCR0() | 0x10000));

            //Native.Invlpg();
        }

        public unsafe static void InitialKernelProtect_MakeWritable_ByRegion(uint startVirtAddr, uint endVirtAddr)
        {
            InitialKernelProtect_MakeWritable_BySize(startVirtAddr, endVirtAddr - startVirtAddr);
        }

        public unsafe static void InitialKernelProtect_MakeWritable_ByMapType(BootInfoMemoryType type)
        {
            var mm = BootInfo.GetMap(type);
            InitialKernelProtect_MakeWritable_BySize(mm->Start, mm->Size);
        }

        public unsafe static void InitialKernelProtect_MakeWritable_BySize(uint virtAddr, uint size)
        {
            if (!UseKernelWriteProtection)
                return;

            //KernelMessage.WriteLine("Unprotect Memory: Start={0:X}, End={1:X}", virtAddr, virtAddr + size);
            var pages = KMath.DivCeil(size, 4096);
            for (var i = 0; i < pages; i++)
            {
                var entry = PageTable.GetTableEntry(virtAddr);
                entry->Writable = true;

                virtAddr += 4096;
            }

            Native.SetCR3(PageTable.AddrPageDirectory);
        }

    }

}
