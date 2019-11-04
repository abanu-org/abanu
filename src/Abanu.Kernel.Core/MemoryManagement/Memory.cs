// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using Abanu.Kernel.Core.Boot;
using Abanu.Kernel.Core.PageManagement;
using Mosa.Runtime.x86;

namespace Abanu.Kernel.Core.MemoryManagement
{

    public static class Memory
    {

        public static unsafe void Setup()
        {
            kmallocAllocator = new KernelAllocator();

            // TODO: CRITICAL: KMath.AlignValueCeil --> DivCeil!
            var ptr = (byte*)VirtualPageManager.AllocatePages(KMath.AlignValueCeil(Allocator.headSize, 4096));
            for (var i = 0; i < Allocator.headSize; i++)
                *(ptr + i) = 0;
            kmallocAllocator.List_heads = (malloc_meta**)ptr;
            ManagedMemoy.UseAllocator = true;
            KernelMessage.WriteLine("EarlyBootBytesUsed: {0} bytes", ManagedMemoy.EarlyBootBytesUsed);

            KernelMessage.WriteLine("Memory free: {0} MB", PhysicalPageManager.FreePages * 4096 / 1024 / 1024);
        }

        private static uint RequiredPagesForSize(USize size)
        {
            return KMath.DivCeil(size, 4096);
        }

        private static void FreeRawVirtualMemory(uint size)
        {
            KernelMessage.WriteLine("NotImplemented");
        }

        private static Allocator kmallocAllocator;

        public static unsafe Addr Allocate(USize n)
        {
            return kmallocAllocator.malloc(n);
        }

        /// <summary>
        /// kmalloc is the normal method of allocating memory for objects smaller than page size in the kernel.
        /// </summary>
        public static unsafe Addr Allocate(USize n, GFP flags)
        {
            //if (VirtualPageManager.LockCount != 0)
            //{
            //    Serial.Write(Serial.COM1, (byte)'~');
            //}

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

        public static Addr AllocateVirtual(USize size)
        {
            return Addr.Zero;
        }

        /// <summary>
        /// free previously allocated memory
        /// </summary>
        public static unsafe void Free(Addr address)
        {
            kmallocAllocator.free(address);
        }

        /// <summary>
        /// free previously allocated memory
        /// </summary>
        public static unsafe void Free(IntPtr address)
        {
            kmallocAllocator.free((void*)address);
        }

        public static unsafe void FreeObject(object obj)
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

        public struct Pgprot_t
        {
        }

        public static unsafe Addr MapVirtualPages(Page* pages, uint count, ulong flags, Pgprot_t protection)
        {
            return Addr.Zero;
        }

        public static unsafe void InitialKernelProtect()
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

            PageTable.KernelTable.SetWritable(BootInfoMemoryType.GDT);
            PageTable.KernelTable.SetWritable(BootInfoMemoryType.PageTable);
            PageTable.KernelTable.SetWritable(BootInfoMemoryType.InitialStack);
            //PageTable.KernelTable.InitialKernelProtect_MakeWritable_ByMapType(BootInfoMemoryType.KernelElfVirt);
            PageTable.KernelTable.SetWritable(BootInfoMemoryType.KernelBssSegment);
            PageTable.KernelTable.SetWritable(BootInfoMemoryType.KernelDataSegment);
            //PageTable.KernelTable.InitialKernelProtect_MakeWritable_ByMapType(BootInfoMemoryType.KernelROdataSegment);
            PageTableExtensions.SetWritable(PageTable.KernelTable, Address.GCInitialMemory, Address.GCInitialMemorySize);

            //KernelMessage.WriteLine("Reload CR3 to {0:X8}", PageTable.AddrPageDirectory);
            PageTable.KernelTable.Flush();

            //KernelMessage.WriteLine("Set CR0.WP");
            PageTable.KernelTable.EnableKernelWriteProtection();
        }

        private static unsafe void SetInitialExecutionProtection()
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

    }

}
