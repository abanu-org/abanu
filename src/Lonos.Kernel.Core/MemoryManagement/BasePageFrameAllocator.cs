// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using Lonos.Kernel.Core.Boot;
using Lonos.Kernel.Core.Devices;
using Lonos.Kernel.Core.Diagnostics;
using Lonos.Kernel.Core.PageManagement;
using Mosa.Runtime;

namespace Lonos.Kernel.Core.MemoryManagement
{

    /// <summary>
    /// A physical page allocator.
    /// </summary>
    public abstract unsafe class BasePageFrameAllocator : IPageFrameAllocator
    {

        public Page* AllocatePages(uint pages)
        {
            return Allocate(pages);
        }

        public Page* AllocatePage()
        {
            var p = Allocate(1);
            return p;
        }

        public void Free(Page* page)
        {
            Free(page->Address);
        }

        public uint FreePages { get; set; }

        public abstract uint TotalPages { get; }

        private void DumpPage(Page* p)
        {
            KernelMessage.WriteLine("pNum {0}, phys {1:X8} status {2} struct {3:X8} structPage {4}", p->PageNum, p->Address, (uint)p->Status, (uint)p, (uint)p / 4096);
        }

        public void Dump()
        {
            var sb = new StringBuffer();

            for (uint i = 0; i < TotalPages; i++)
            {
                var p = GetPageByNum(i);
                if (i % 64 == 0)
                {
                    sb.Append("\nIndex={0} Page {1} at {2:X8}, PageStructAddr={3:X8}: ", i, p->PageNum, p->Address, (uint)p);
                    sb.WriteTo(DeviceManager.Serial1);
                    sb.Clear();
                }
                sb.Append((int)p->Status);
                sb.WriteTo(DeviceManager.Serial1);
                sb.Clear();
            }
        }

        public Page* GetPageByAddress(Addr physAddr)
        {
            return GetPageByNum((uint)physAddr / PageSize);
        }

        public abstract Page* GetPageByNum(uint pageNum);

        private static Page* lastAllocatedPage;

        /// <summary>
        /// Allocate a physical page from the free list
        /// </summary>
        /// <returns>The page</returns>
        private Page* Allocate(uint num)
        {
            lock (this)
            {
                if (num == 0)
                {
                    KernelMessage.WriteLine("Requesting zero pages");
                    return null;
                }
                else if (num > 1 && KConfig.TracePageAllocation)
                {
                    KernelMessage.WriteLine("Requesting {0} pages", num);
                }

                //KernelMessage.WriteLine("Request {0} pages...", num);

                uint statBlocks = 0;
                uint statFreeBlocks = 0;
                int statMaxBlockPages = 0;
                uint statRangeChecks = 0;

                uint cnt = 0;

                if (lastAllocatedPage == null)
                    lastAllocatedPage = GetPageByNum(0);

                Page* p = lastAllocatedPage->Next;
                while (true)
                {
                    statBlocks++;

                    if (p == null)
                        p = GetPageByNum(0);

                    if (p->Status == PageStatus.Free)
                    {
                        statFreeBlocks++;
                        var head = p;

                        // Found free Page. Check now free range.
                        for (var i = 0; i < num; i++)
                        {
                            statRangeChecks++;
                            statMaxBlockPages = Math.Max(statMaxBlockPages, i);

                            if (p == null)
                                break; // Reached end. SorRange is incomplete
                            if (p->Status != PageStatus.Free) // Used -> so we can abort the searach
                                break;

                            if (i == num - 1)
                            { // all loops successful. So we found our range.

                                head->Tail = p;
                                head->PagesUsed = num;
                                p = head;
                                for (var n = 0; n < num; n++)
                                {
                                    if (p->Status != PageStatus.Free)
                                        Panic.Error("Page is not Free. PageFrame Array corrupted?");

                                    p->Status = PageStatus.Used;
                                    p->Head = head;
                                    p->Tail = head->Tail;
                                    p = p->Next;
                                    FreePages--;
                                }
                                lastAllocatedPage = p;

                                //KernelMessage.WriteLine("Allocated from {0:X8} to {1:X8}", (uint)head->PhysicalAddress, (uint)head->Tail->PhysicalAddress + 4096 - 1);

                                //if (head->PhysicalAddress == 0x01CA4000)
                                //{
                                //    KernelMessage.WriteLine("DEBUG-MARKER 2");
                                //    DumpPage(head);
                                //}

                                return head;
                            }

                            p = p->Next;
                        }

                    }

                    if (p->Tail != null)
                        p = p->Tail;

                    p = p->Next;
                    if (++cnt > TotalPages)
                        break;
                }

                KernelMessage.WriteLine("Blocks={0} FreeBlocks={1} MaxBlockPages={2} RangeChecks={3} cnt={4}", statBlocks, statFreeBlocks, (uint)statMaxBlockPages, statRangeChecks, cnt);
                Dump();
                Panic.Error("PageFrameAllocator: Could not allocate " + num + " Pages.");
                return null;
            }
        }

        /// <summary>
        /// Releases a page to the free list
        /// </summary>
        private void Free(Addr address)
        {
            lock (this)
            {
                var p = GetPageByAddress(address);
                if (p->Free)
                    return;

                var num = p->PagesUsed;

                for (var n = 0; n < num; n++)
                {
                    p->Status = PageStatus.Used;
                    p->PagesUsed = 0;
                    p->Head = null;
                    p->Tail = null;
                    p = p->Next;
                    FreePages++;
                }
            }
        }

        /// <summary>
        /// Gets the size of a single memory page.
        /// </summary>
        public static uint PageSize => 4096;

    }
}
