// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;

namespace Lonos.Kernel.Core.MemoryManagement.PageAllocators
{
    public unsafe class MultiAllocator : IPageFrameAllocator
    {

        public IPageFrameAllocator[] Allocators;

        public void Initialize(IPageFrameAllocator[] allocators)
        {
            Allocators = allocators;
        }

        public uint TotalPages
        {
            get
            {
                uint n = 0;
                for (var i = 0; i < Allocators.Length; i++)
                    n += Allocators[i].TotalPages;
                return n;
            }
        }

        public uint FreePages
        {
            get
            {
                uint n = 0;
                for (var i = 0; i < Allocators.Length; i++)
                    n += Allocators[i].FreePages;
                return n;
            }
        }

        public MemoryRegion Region => throw new NotSupportedException("Not supported");

        public AddressSpaceKind AddressSpaceKind => Allocators[0].AddressSpaceKind;

        private IPageFrameAllocator GetAllocatorByPage(Page* p)
        {
            for (var i = 0; i < Allocators.Length; i++)
                if (Allocators[i].ContainsPage(p))
                    return Allocators[i];
            return null;
        }

        private IPageFrameAllocator GetAllocatorByAddr(Addr addr)
        {
            for (var i = 0; i < Allocators.Length; i++)
                if (Allocators[i].Region.Contains(addr))
                    return Allocators[i];
            return null;
        }

        private IPageFrameAllocator GetAllocatorByPageNum(uint num)
        {
            var addr = num * 4096;
            for (var i = 0; i < Allocators.Length; i++)
                if (Allocators[i].Region.Contains(addr))
                    return Allocators[i];
            return null;
        }

        private IPageFrameAllocator GetAllocatorByPageIndex(uint index)
        {
            uint idx = index;
            for (var i = 0; i < Allocators.Length; i++)
            {
                var page = Allocators[i].GetPageByIndex(idx);
                if (page != null)
                    return Allocators[i];
                idx += Allocators[i].TotalPages;
            }
            return null;
        }

        public Page* AllocatePage(AllocatePageOptions options = default)
        {
            return Allocators[0].AllocatePage(options);
        }

        public Page* AllocatePages(uint pages, AllocatePageOptions options = default)
        {
            if (pages <= 512)
                return Allocators[0].AllocatePages(pages, options);
            else
                return Allocators[1].AllocatePages(pages, options);
        }

        public void Free(Page* page)
        {
            GetAllocatorByPage(page).Free(page);
        }

        public uint GetAddress(Page* page)
        {
            return GetAllocatorByPage(page).GetAddress(page);
        }

        public Page* GetPageByAddress(Addr addr)
        {
            return GetAllocatorByAddr(addr).GetPageByAddress(addr);
        }

        public Page* GetPageByIndex(uint pageIndex)
        {
            return GetAllocatorByPageIndex(pageIndex).GetPageByIndex(pageIndex);
        }

        public Page* GetPageByNum(uint pageNum)
        {
            return GetAllocatorByPageNum(pageNum).GetPageByNum(pageNum);
        }

        public uint GetPageNum(Page* page)
        {
            return GetAllocatorByPage(page).GetPageNum(page);
        }

        public Page* NextPage(Page* page)
        {
            var idx = GetPageIndex(page);
            return GetPageByIndex(idx + 1);
        }

        public uint GetPageIndex(Page* page)
        {
            return GetAllocatorByPage(page).GetPageIndex(page);
        }

        public uint GetPageIndex(Addr addr)
        {
            return GetAllocatorByAddr(addr).GetPageIndex(addr);
        }

        public bool ContainsPage(Page* page)
        {
            for (var i = 0; i < Allocators.Length; i++)
                if (Allocators[i].ContainsPage(page))
                    return true;
            return false;
        }

        public Page* NextCompoundPage(Page* page)
        {
            return GetAllocatorByPage(page).NextCompoundPage(page);
        }

        public void SetTraceOptions(PageFrameAllocatorTraceOptions options)
        {
            for (var i = 0; i < Allocators.Length; i++)
                Allocators[i].SetTraceOptions(options);
        }

        public ulong Requests
        {
            get
            {
                ulong n = 0;
                for (var i = 0; i < Allocators.Length; i++)
                    n += Allocators[i].Requests;
                return n;
            }
        }

        public ulong Releases
        {
            get
            {
                ulong n = 0;
                for (var i = 0; i < Allocators.Length; i++)
                    n += Allocators[i].Releases;
                return n;
            }
        }

        private string _DebugName;
        public string DebugName
        {
            get { return _DebugName; }
            set { _DebugName = value; }
        }

    }

}
