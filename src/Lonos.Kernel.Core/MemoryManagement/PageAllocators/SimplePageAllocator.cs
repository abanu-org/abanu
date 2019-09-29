// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lonos.Kernel.Core.Diagnostics;

namespace Lonos.Kernel.Core.MemoryManagement.PageAllocators
{
    public unsafe class SimplePageAllocator : IPageFrameAllocator
    {

        private Page* Pages;
        private uint FirstPageNum;
        internal string DebugName = "";

        public void Initialize(MemoryRegion region, Page* pages, AddressSpaceKind addressSpaceKind)
        {
            KernelMessage.WriteLine("Init SimplePageAllocator");
            AddressSpaceKind = addressSpaceKind;
            _Region = region;
            Pages = pages;
            FirstPageNum = region.Start / 4096;
            _FreePages = region.Size / 4096;
            _TotalPages = region.Size / 4096;
            var addr = region.Start;
            for (var i = 0; i < _TotalPages; i++)
            {
                Pages[i].Address = addr;
                addr += 4096;
            }
        }

        private uint _TotalPages;
        public uint TotalPages => _TotalPages;

        private uint _FreePages;
        public uint FreePages => _FreePages;

        private MemoryRegion _Region;
        public MemoryRegion Region => _Region;

        public AddressSpaceKind AddressSpaceKind { get; private set; }

        public unsafe Page* AllocatePage(AllocatePageOptions options = AllocatePageOptions.Default)
        {
            return AllocatePages(1, options);
        }

        public unsafe Page* AllocatePages(uint pages, AllocatePageOptions options = AllocatePageOptions.Default)
        {
            if (pages > _FreePages)
            {
                Panic.Error("Out of Memory");
                return null;
            }

            Page* page;
            if (AddressSpaceKind == AddressSpaceKind.Virtual || (options & AllocatePageOptions.Continuous) == AllocatePageOptions.Continuous)
                page = AllocatePagesContinuous(pages, options);
            else
                page = AllocatePagesNormal(pages, options);

            if (page == null)
            {
                KernelMessage.WriteLine("DebugName: {0}", DebugName);
                KernelMessage.WriteLine("Free pages: {0:X8}, Requested: {1:X8}, Options {2}", FreePages, pages, (uint)options);
                Panic.Error("Out of Memory");
            }

            return page;
        }

        private unsafe Page* AllocatePagesNormal(uint pages, AllocatePageOptions options = AllocatePageOptions.Default)
        {
            Page* prevHead = null;
            Page* firstHead = null;
            var pagesFound = 0;
            for (var i = 0; i < _TotalPages; i++)
            {
                var page = &Pages[i];
                if (!InUse(page))
                {
                    if (firstHead == null)
                        firstHead = page;

                    if (prevHead != null)
                        SetNext(prevHead, page);
                    SetInUse(page);

                    pagesFound++;
                    _FreePages--;

                    if (pagesFound >= pages)
                        return firstHead;

                    prevHead = page;
                }
            }

            return null;
        }

        private unsafe Page* AllocatePagesContinuous(uint pages, AllocatePageOptions options = AllocatePageOptions.Default)
        {
            for (var i = 0; i < pages; i++)
            {
                if (i + pages >= _TotalPages)
                    return null;

                var head = &Pages[i];
                if (!InUse(head))
                {
                    var foundContinuous = true;
                    for (var n = 1; n < pages; n++)
                    {
                        if (InUse(&Pages[++i]))
                        {
                            foundContinuous = false;
                            break;
                        }
                    }

                    if (!foundContinuous)
                        continue;

                    var p = head;
                    for (var n = 0; n < pages; n++)
                    {
                        SetInUse(p);

                        if (n == pages - 1)
                            SetNext(p, null);
                        else
                            SetNext(p, p + 1);

                        p++;
                    }

                    _FreePages -= pages;

                    return head;
                }
            }

            return null;
        }

        private static bool InUse(Page* page) => (page->Flags & (1u << 1)) != 0;
        private static void SetInUse(Page* page) => page->Flags |= 1u << 1;
        private static void ClearInUse(Page* page) => page->Flags &= ~(1u << 1);

        // Don't be confused: Reuse of FirstPage field of buddy allocator just to save space
        private static void SetNext(Page* page, Page* next) => page->FirstPage = next;

        public unsafe bool ContainsPage(Page* page)
        {
            return Region.Contains(page->Address);
        }

        public unsafe void Free(Page* page)
        {
            var next = page;
            while (next != null)
            {
                SetNext(page, null);
                ClearInUse(page);
                _FreePages++;
                next = NextCompoundPage(page);
            }
        }

        public unsafe uint GetAddress(Page* page)
        {
            return page->Address;
        }

        public unsafe Page* GetPageByAddress(Addr addr)
        {
            return GetPageByNum(addr / 4096);
        }

        public unsafe Page* GetPageByIndex(uint pageIndex)
        {
            if (pageIndex >= _TotalPages)
                return null;

            return &Pages[pageIndex];
        }

        public unsafe Page* GetPageByNum(uint pageNum)
        {
            return GetPageByIndex(pageNum - FirstPageNum);
        }

        public unsafe uint GetPageIndex(Page* page)
        {
            return GetPageNum(page) - FirstPageNum;
        }

        public uint GetPageIndex(Addr addr)
        {
            return (addr / 4096) - FirstPageNum;
        }

        public unsafe uint GetPageNum(Page* page)
        {
            return page->Address / 4096;
        }

        public unsafe Page* NextPage(Page* page)
        {
            return GetPageByIndex(GetPageIndex(page) + 1);
        }

        public Page* NextCompoundPage(Page* page)
        {
            KernelMessage.WriteLine("NextCompoundPage: {0:X8}, {1:X8}", GetAddress(page), (uint)page->FirstPage);
            return page->FirstPage;
        }
    }

}
