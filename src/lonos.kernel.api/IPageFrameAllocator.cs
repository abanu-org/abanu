using System;

namespace Lonos.Kernel.Core
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
        Page* GetPhysPage(Addr addr);
    }

}
