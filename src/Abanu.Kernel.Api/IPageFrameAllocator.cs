// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using Abanu.Kernel.Core.PageManagement;

namespace Abanu.Kernel.Core
{

    [Flags]
    public enum AddressSpaceKind
    {
        None = 0,
        Physical = 1,
        Virtual = 2,
        Both = Physical | Virtual,
    }

    public unsafe interface IPageFrameAllocator
    {
        Page* AllocatePages(uint pages, AllocatePageOptions options = default);

        Page* AllocatePage(AllocatePageOptions options = default);

        void Free(Page* page);

        Page* GetPageByAddress(Addr addr);
        uint GetAddress(Page* page);

        Page* GetPageByNum(uint pageNum);
        uint GetPageNum(Page* page);
        Page* GetPageByIndex(uint pageIndex);
        uint TotalPages { get; }

        uint FreePages { get; }
        ulong Requests { get; }
        ulong Releases { get; }
        uint MaxPagesPerAllocation { get; }
        uint CriticalLowPages { get; }
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

        void SetTraceOptions(PageFrameAllocatorTraceOptions options);

        string DebugName { get; set; }

    }

    public struct AllocatePageOptions
    {
        public bool Continuous;
        public string DebugName;
        public PageAllocationPool Pool;
        //public IPageTable Target1;
        //public IPageTable Target2;

        public static AllocatePageOptions Default;

    }

    public enum PageAllocationPool
    {
        Normal,
        Identity,
        Global,
    }

    public struct PageFrameAllocatorTraceOptions
    {
        public bool Enabled;
        public uint MinPages;
    }

}
