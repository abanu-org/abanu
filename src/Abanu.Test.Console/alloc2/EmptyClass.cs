// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using Abanu.Kernel.Core;

#pragma warning disable

namespace Abanu.Test.Console.alloc2
{

    unsafe struct list_head
    {
        public list_head* Next;
        public list_head* Prev;
    }

    struct page
    {
        public uint Flags;
        public byte Private_;
        public uint _count;
        public list_head Lru;
    }

    unsafe struct zone
    {
        public ulong Free_pages;
        public free_area* Free_area; //[MAX_ORDER]
    }

    struct free_area
    {
        public list_head Free_list;
        public ulong Nr_free;
    }

    public unsafe class EmptyClass
    {
        public EmptyClass()
        {
        }

        const byte MAX_ORDER = 11;

        const byte PG_reserved = 11;
        const byte PG_private = 12;

        static bool page_is_buddy(page* page, int order)
        {
            if (PagePrivate(page) &&
                (page_order(page) == order) &&
                !PageReserved(page) &&
                 page_count(page) == 0)
                return true;
            return false;
        }

        static bool PagePrivate(page* page)
        {
            return page->Flags.IsBitSet(PG_private);
        }

        static uint page_order(page* page)
        {
            return page->Private_;
        }

        static bool PageReserved(page* page)
        {
            return page->Flags.IsBitSet(PG_reserved);
        }

        static uint page_count(page* page)
        {
            return page->_count + 1;
        }

        static page* buffered_rmqueue(zone* zone, int order, int gfp_flags)
        {
            ulong flags;
            page* page = null;
            //int cold = !!(gfp_flags & __GFP_COLD);

            //if (order == 0)
            //{
            //    per_cpu_pages* pcp;

            //    pcp = &zone->pageset[get_cpu()].pcp[cold];
            //    local_irq_save(flags);
            //    if (pcp->count <= pcp->low)
            //        pcp->count += rmqueue_bulk(zone, 0,
            //                    pcp->batch, &pcp->list);
            //    if (pcp->count)
            //    {
            //        page = list_entry(pcp->list.next, page, lru);
            //        list_del(&page->lru);
            //        pcp->count--;
            //    }
            //    local_irq_restore(flags);
            //    put_cpu();
            //}

            //if (page == null)
            //{
            //    //spin_lock_irqsave(&zone->lock_, flags);
            //    page = __rmqueue(zone, order);
            //    //spin_unlock_irqrestore(&zone->lock_, flags);
            //}

            //if (page != null)
            //{
            //    BUG_ON(bad_range(zone, page));
            //    //mod_page_state_zone(zone, pgalloc, 1 << order);
            //    prep_new_page(page, order);

            //    if (gfp_flags & __GFP_ZERO)
            //        prep_zero_page(page, order, gfp_flags);

            //    if (order && (gfp_flags & __GFP_COMP))
            //        prep_compound_page(page, order);
            //}
            return page;
        }

        static page* __rmqueue(zone* zone, byte order)
        {
            free_area* area;
            byte current_order;
            page* page;

            for (current_order = order; current_order < MAX_ORDER; ++current_order)
            {
                area = zone->Free_area + current_order;
                if (list_empty(&area->Free_list))
                    continue;

                //page = list_entry(area->free_list.next, page, lru);
                page = list_entry__1(area->Free_list.Next);
                list_del(&page->Lru);
                rmv_page_order(page);
                area->Nr_free--;
                zone->Free_pages -= 1UL << order;
                return expand(zone, page, order, current_order, area);
            }

            return null;
        }

        static page* expand(zone* zone, page* page, int low, int high, free_area* area)
        {
            uint size = (uint)1 << (byte)high;

            while (high > low)
            {
                area--;
                high--;
                size >>= 1;
                //BUG_ON(bad_range(zone, &page[size]));
                list_add(&page[size].Lru, &area->Free_list);
                area->Nr_free++;
                set_page_order(&page[size], high);
            }
            return page;
        }

        static void list_add(list_head* new_, list_head* head)
        {
            __list_add(new_, head, head->Next);
        }

        static void __list_add(
            list_head* new_,
            list_head* prev,
            list_head* next)
        {
            next->Prev = new_;
            new_->Next = next;
            new_->Prev = prev;
            prev->Next = new_;
        }

        static void rmv_page_order(page* page)
        {
            page->Flags = page->Flags.ClearBit(PG_private);
            page->Private_ = 0;
        }

        static page* list_entry__1(list_head* ptr)
        {
            var p = (page*)(void*)ptr;
            return (page*)(void*)(&(p->Lru));
        }

        static void set_page_order(page* page, int order)
        {
            page->Private_ = (byte)order;
            page->Flags = page->Flags.SetBit(PG_private);
        }

        /*
    * These are non-NULL pointers that will result in page faults
    * under normal circumstances, used to verify that nobody uses
    * non-initialized list entries.
    */
        private static void* LIST_POISON1 = (void*)0x00100100;
        private static void* LIST_POISON2 = (void*)0x00200200;

        static bool list_empty(list_head* head)
        {
            return head->Next == head;
        }

        static void list_del(list_head* entry)
        {
            __list_del(entry->Prev, entry->Next);
            entry->Next = (list_head*)LIST_POISON1;
            entry->Prev = (list_head*)LIST_POISON2;
        }

        static void __list_del(list_head* prev, list_head* next)
        {
            next->Prev = prev;
            prev->Next = next;
        }
    }
}

