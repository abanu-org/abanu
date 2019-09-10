//using Mosa.Kernel.x86;

namespace lonos.Kernel.Core.MemoryManagement
{
    public static unsafe class PageFrameManager
    {

        private static PageFrameAllocator Default;
        public const uint PageSize = 4096;

        public static void Setup()
        {
            Default = new PageFrameAllocator();
            Default.Setup();
        }

        public static Page* AllocatePages(PageFrameRequestFlags flags, uint pages)
        {
            return Default.AllocatePages(flags, pages);
        }

        public static Page* AllocatePage(PageFrameRequestFlags flags)
        {
            var p = Default.AllocatePage(flags);
            //if (p->PhysicalAddress == 0x01CA4000)
            //    Panic.Error("DEBUG-MARKER");
            return p;
        }

        public static void Free(Page* page)
        {
            Default.Free(page);
        }

        public static Page* GetPhysPage(Addr physAddr)
        {
            return Default.GetPhysPage(physAddr);
        }

        public static Page* GetPageByNum(uint pageNum)
        {
            return Default.GetPageByNum(pageNum);
        }

        public static uint PagesAvailable
        {
            get
            {
                return Default.PagesAvailable;
            }
        }

    }
}
