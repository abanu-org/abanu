// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;

namespace Lonos.Kernel.Core
{

    [Flags]
    public enum AllocatePageOptions
    {
        Default = 0,
        Continuous = 1,
    }

    public enum AddressSpaceKind
    {
        Physical = 0,
        Virtual = 1,
    }

    public unsafe interface IPageFrameAllocator
    {
        Page* AllocatePages(uint pages, AllocatePageOptions options = AllocatePageOptions.Default);

        Page* AllocatePage(AllocatePageOptions options = AllocatePageOptions.Default);

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
        Page* NextCompoundPage(Page* page);
        uint GetPageIndex(Page* page);
        uint GetPageIndex(Addr addr);

        bool ContainsPage(Page* page);
        //bool ContainsAddr(Addr addr);
        //bool ContainsPageNum(uint pageNum);
        //bool ContainsPageIndex(uint age);

        AddressSpaceKind AddressSpaceKind { get; }

    }

}
