// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;

namespace Lonos.Kernel.Core
{

    public enum PageFrameRequestFlags
    {
        Default,
    }

    public unsafe interface IPageFrameAllocator
    {
        Page* AllocatePages(uint pages);

        Page* AllocatePage();

        void Free(Page* page);

        Page* GetPageByAddress(Addr addr);
        uint GetAddress(Page* page);

        Page* GetPageByNum(uint pageNum);
        uint GetPageNum(Page* page);
        Page* GetPageByIndex(uint pageIndex);
        uint TotalPages { get; }

        uint FreePages { get; }
        MemoryRegion Region { get; }

        Page* NextPage(Page* page);
    }

}
