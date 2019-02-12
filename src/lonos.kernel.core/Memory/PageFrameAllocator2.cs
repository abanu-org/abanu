//using System;
//using Mosa.Runtime;

////using Mosa.Kernel.x86;


//namespace lonos.kernel.core
//{
//    /// <summary>
//    /// A physical page allocator.
//    /// </summary>
//    public static class PageFrameAllocator2
//    {
//        // Start of memory map
//        private static IntPtr map;

//        // Current position in map data structure
//        private static IntPtr at;

//        private static uint totalPages;
//        private static uint totalUsedPages;

//        static KernelMemoryMap kmap;

//        /// <summary>
//        /// Setup the physical page manager
//        /// </summary>
//        public static void Setup()
//        {
//            kmap = KernelMemoryMapManager.Allocate(4 * 1024 * 1024, BootInfoMemoryType.PageFrameAllocator);

//            map = new IntPtr((uint)kmap.Start);
//            at = new IntPtr((uint)kmap.Start);
//            totalPages = 0;
//            totalUsedPages = 0;
//            SetupFreeMemory();
//        }

//        /// <summary>
//        /// Setups the free memory.
//        /// </summary>
//        unsafe static void SetupFreeMemory()
//        {
//            if (!BootInfo.Present)
//                return;

//            uint cnt = 0;

//            for (uint index = 0; index < BootInfo.Header->MemoryMapLength; index++)
//            {
//                var mm = &BootInfo.Header->MemoryMapArray[index];

//                if (mm->Type == BootInfoMemoryType.SystemUsable)
//                {
//                    AddFreeMemory(cnt++, mm->Start, mm->Size);
//                }
//            }
//        }

//        /// <summary>
//        /// Adds the free memory.
//        /// </summary>
//        /// <param name="cnt">The count.</param>
//        /// <param name="start">The start.</param>
//        /// <param name="size">The size.</param>
//        private static void AddFreeMemory(uint cnt, uint start, uint size)
//        {
//            KernelMessage.Path("PageFrameAllocator", "Add Start={0:X8}, Size:{1:X8}", start, size);
//            if ((start > Address.MaximumMemory) || (start + size < Address.ReserveMemory))
//                return;

//            // Normalize
//            uint normstart = (start + PageSize - 1) & ~(PageSize - 1);
//            uint normend = (start + size) & ~(PageSize - 1);
//            uint normsize = normend - normstart;

//            // Adjust if memory below is reserved
//            if (normstart < Address.ReserveMemory)
//            {
//                if ((normstart + normsize) < Address.ReserveMemory)
//                    return;

//                normsize = (normstart + normsize) - Address.ReserveMemory;
//                normstart = Address.ReserveMemory;
//            }

//            // Populate free table
//            for (uint mem = normstart; mem < normstart + normsize; mem = mem + PageSize, at = at + 4)
//            {
//                Intrinsic.Store32(at, mem);
//            }

//            at -= 4;
//            totalPages += (normsize / PageSize);
//        }

//        /// <summary>
//        /// Allocate a physical page from the free list
//        /// </summary>
//        /// <returns>The page</returns>
//        public static IntPtr Allocate()
//        {
//            if (at == map)
//                return IntPtr.Zero; // out of memory

//            totalUsedPages++;
//            var avail = Intrinsic.LoadPointer(at);
//            at -= 4;

//            // Clear out memory
//            Mosa.Runtime.Internal.MemoryClear(avail, PageSize);

//            return avail;
//        }

//        /// <summary>
//        /// Releases a page to the free list
//        /// </summary>
//        /// <param name="address">The address.</param>
//        public static void Free(IntPtr address)
//        {
//            totalUsedPages--;
//            at += 4;
//            Intrinsic.Store32(at, address.ToInt32());
//        }

//        /// <summary>
//        /// Retrieves the size of a single memory page.
//        /// </summary>
//        public static uint PageSize { get { return 4096; } }

//        /// <summary>
//        /// Retrieves the amount of total physical memory pages available in the system.
//        /// </summary>
//        public static uint TotalPages { get { return totalPages; } }

//        /// <summary>
//        /// Retrieves the amount of number of physical pages in use.
//        /// </summary>
//        public static uint TotalPagesInUse { get { return totalUsedPages; } }


//        public static uint PagesAvailable
//        {
//            get
//            {
//                return TotalPages - TotalPagesInUse;
//            }
//        }

//    }
//}
