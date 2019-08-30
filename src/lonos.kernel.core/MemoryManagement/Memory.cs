using System;
using lonos.Kernel.Core.Boot;
using lonos.Kernel.Core.PageManagement;
using Mosa.Kernel.x86;
using Mosa.Runtime.x86;

namespace lonos.Kernel.Core.MemoryManagement
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
            KernelMessage.WriteLine("EarlyBootBytesUsed: {0} bytes", ManagedMemoy.EarlyBootBytesUsed);

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

        public unsafe static Addr Allocate(USize n)
        {
            return kmallocAllocator.malloc(n);
        }

        /// <summary>
        /// kmalloc is the normal method of allocating memory for objects smaller than page size in the kernel.
        /// </summary>
        public unsafe static Addr Allocate(USize n, GFP flags)
        {
            // var sb = new StringBuffer();
            // sb.Append("Alloc: Size: {0:X8}", (uint)n);
            var addr = kmallocAllocator.malloc(n);
            // sb.Append("Alloc: Addr: {0}", (uint)addr);
            // sb.WriteTo(Devices.Serial1);

            return addr;
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

        public unsafe static void InitialKernelProtect()
        {
            SetInitialWriteProtection();
            SetInitialExecutionProtection();
        }

        private static void SetInitialWriteProtection()
        {
            if (!KConfig.UseKernelMemoryProtection)
                return;

            //KernelMessage.WriteLine("Unset CR0.WP");
            PageTable.KernelTable.DisableKernelWriteProtection();

            PageTable.KernelTable.SetKernelWriteProtectionForAllInitialPages();

            InitialKernelProtect_MakeWritable_ByMapType(BootInfoMemoryType.GDT);
            InitialKernelProtect_MakeWritable_ByMapType(BootInfoMemoryType.PageTable);
            InitialKernelProtect_MakeWritable_ByMapType(BootInfoMemoryType.InitialStack);
            //InitialKernelProtect_MakeWritable_ByMapType(BootInfoMemoryType.KernelElfVirt);
            InitialKernelProtect_MakeWritable_ByMapType(BootInfoMemoryType.KernelBssSegment);
            InitialKernelProtect_MakeWritable_ByMapType(BootInfoMemoryType.KernelDataSegment);
            //InitialKernelProtect_MakeWritable_ByMapType(BootInfoMemoryType.KernelROdataSegment);
            InitialKernelProtect_MakeWritable_BySize(Address.GCInitialMemory, Address.GCInitialMemorySize);

            //KernelMessage.WriteLine("Reload CR3 to {0:X8}", PageTable.AddrPageDirectory);
            PageTable.KernelTable.Flush();

            //KernelMessage.WriteLine("Set CR0.WP");
            PageTable.KernelTable.EnableKernelWriteProtection();
        }

        private unsafe static void SetInitialExecutionProtection()
        {
            if (KConfig.UseExecutionProtection)
            {
                var code = BootInfo.GetMap(BootInfoMemoryType.KernelTextSegment);
                var codeReg = new LinkedMemoryRegion(new MemoryRegion(code->Start, code->Size));
                //var otherReg = new LinkedMemoryRegion(new MemoryRegion(0, 10124 * 1024 * 60), &codeReg);
                //var otherReg = new LinkedMemoryRegion(new MemoryRegion(0, 10124 * 1024 * 60), &codeReg);

                PageTable.KernelTable.SetExecutionProtectionForAllInitialPages(&codeReg);
                //InitialKernelProtect_MakeExecutable_ByMapType(BootInfoMemoryType.KernelTextSegment);
            }
        }

        public unsafe static void InitialKernelProtect_MakeWritable_ByMapType(BootInfoMemoryType type)
        {
            var mm = BootInfo.GetMap(type);
            InitialKernelProtect_MakeWritable_BySize(mm->Start, mm->Size);
        }

        public unsafe static void InitialKernelProtect_MakeWritable_BySize(uint virtAddr, uint size)
        {
            if (!KConfig.UseKernelMemoryProtection)
                return;

            PageTable.KernelTable.SetKernelWriteProtectionForRegion(virtAddr, size);
        }

        public unsafe static void InitialKernelProtect_MakeExecutable_ByMapType(BootInfoMemoryType type)
        {
            var mm = BootInfo.GetMap(type);
            InitialKernelProtect_MakeExecutable_BySize(mm->Start, mm->Size);
        }

        public unsafe static void InitialKernelProtect_MakeExecutable_BySize(uint virtAddr, uint size)
        {
            if (!KConfig.UseKernelMemoryProtection)
                return;

            PageTable.KernelTable.SetExecutableForRegion(virtAddr, size);
        }

    }

}
