using System;

namespace lonos.kernel.core
{

    public enum PageFrameRequestFlags
    {
        Default
    }

    public unsafe interface IPageFrameAllocator
    {
        Page* AllocatePages(PageFrameRequestFlags flags, uint pages);
        Page* AllocatePage(PageFrameRequestFlags flags);
        void Free(Page* page);
        Page* GetPage(Addr addr);
    }

}
