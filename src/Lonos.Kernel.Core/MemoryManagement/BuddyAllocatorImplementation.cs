// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Lonos.CTypes;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable SA1400 // Access modifier should be declared
#pragma warning disable SA1307 // Accessible fields should begin with upper-case letter
#pragma warning disable CA1822 // Mark members as static
#pragma warning disable SA1025 // Code should not contain multiple whitespace in a row
#pragma warning disable SA1502 // Element should not be on a single line
#pragma warning disable SA1119 // Statement should not use unnecessary parenthesis
#pragma warning disable SA1120 // Comments should contain text
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
#pragma warning disable SA1117 // Parameters should be on same line or separate lines
#pragma warning disable SA1116 // Split parameters should start on line after declaration

namespace Lonos.Kernel.Core.MemoryManagement
{
    internal static unsafe class BuddyAllocatorImplementation
    {

        public const byte BUDDY_PAGE_SHIFT = 12;
        public const uint BUDDY_PAGE_SIZE = 1 << (byte)BUDDY_PAGE_SHIFT;
        public const byte BUDDY_MAX_ORDER = 9;

        enum pageflags : byte
        {
            PG_head, // not in the buddy system, the first page
            PG_tail, // Not in the buddy system, pages outside the homepage
            PG_buddy, // in the buddy system
        }

        public struct free_area
        {
            public list_head free_list;
            public uint nr_free;
        }

        public unsafe struct mem_zone
        {
            public uint page_num;
            public uint page_size;
            public Page* first_page;
            public uint start_addr;
            public uint end_addr;

            // TODO: Init
            public free_area* free_area; //[BUDDY_MAX_ORDER];
        }

        // The page is divided into two categories: one is a single page(zero page).
        // One type is a compound page.
        // The first of the combined pages is head and the rest is tail.

        static void __SetPageHead(Page* page)
        {
            page->flags |= (1U << (byte)pageflags.PG_head);
        }

        static void __SetPageTail(Page* page)
        {
            page->flags |= (1U << (byte)pageflags.PG_tail);
        }

        static void __SetPageBuddy(Page* page)
        {
            page->flags |= (1U << (byte)pageflags.PG_buddy);
        }
        /**/
        static void __ClearPageHead(Page* page)
        {
            page->flags &= ~(1U << (byte)pageflags.PG_head);
        }

        static void __ClearPageTail(Page* page)
        {
            page->flags &= ~(1U << (byte)pageflags.PG_tail);
        }

        static void __ClearPageBuddy(Page* page)
        {
            page->flags &= ~(1U << (byte)pageflags.PG_buddy);
        }
        /**/
        static bool PageHead(Page* page)
        {
            return (page->flags & (1U << (byte)pageflags.PG_head)) != 0;
        }

        static bool PageTail(Page* page)
        {
            return (page->flags & (1U << (byte)pageflags.PG_tail)) != 0;
        }

        static bool PageBuddy(Page* page)
        {
            return (page->flags & (1U << (byte)pageflags.PG_buddy)) != 0;
        }

        // Set the page's order and PG_buddy flags

        static void set_page_order_buddy(Page* page, byte order)
        {
            page->order = order;
            __SetPageBuddy(page);
        }

        static void rmv_page_order_buddy(Page* page)
        {
            page->order = 0;
            __ClearPageBuddy(page);
        }

        // Find buddy page

        static uint __find_buddy_index(uint page_idx, byte order)
        {
            return (page_idx ^ (1U << order));
        }

        static uint __find_combined_index(uint page_idx, byte order)
        {
            return (page_idx & ~(1U << order));
        }

        //---

        // The Linux kernel records the order of the combined page in the prev pointer of the second page.
        // This system records the order of the combined page in the page->order field of the first page.

        static byte compound_order(Page* page)
        {
            if (!PageHead(page))
                return 0; // single page
            return page->order;
        }

        static void set_compound_order(Page* page, byte order)
        {
            //page[1].lru.prev = (void *)order;
            page->order = order;
        }

        static void BUDDY_BUG(string msg, uint value = 0)
        {
            //printf("BUDDY_BUG in %s, %d.\n", f, line);
            //System.Console.WriteLine(msg);
            KernelMessage.Path("Allocator", msg, value);
            //assert(0);
        }

        static void BUDDY_TRACE(string msg, uint value = 0)
        {
            //printf("BUDDY_BUG in %s, %d.\n", f, line);
            //System.Console.WriteLine(msg);
            KernelMessage.Path("Allocator", msg, value);
            //assert(0);
        }

        // print buddy system status
        //void dump_print(struct mem_zone * zone);
        //void dump_print_dot(struct mem_zone * zone);

        //---###---

        public static void buddy_system_init(mem_zone* zone,
                       Page* start_page,
                       uint start_addr,
                       uint page_num)
        {
            uint i;
            Page* page = null;
            free_area* area = null;
            // init memory zone
            zone->page_num = page_num;
            zone->page_size = BUDDY_PAGE_SIZE;
            zone->first_page = start_page;
            zone->start_addr = start_addr;
            zone->end_addr = start_addr + (page_num * BUDDY_PAGE_SIZE);
            // TODO: init zone->lock
            // init each area
            for (i = 0; i < BUDDY_MAX_ORDER; i++)
            {
                area = zone->free_area + i;
                list_head.INIT_LIST_HEAD(&area->free_list);
                area->nr_free = 0;
            }
            memset((byte*)start_page, 0, page_num * (uint)sizeof(Page));
            // init and free each page
            for (i = 0; i < page_num; i++)
            {
                page = zone->first_page + i;
                list_head.INIT_LIST_HEAD(&page->lru);
                // TODO: init page->lock
                buddy_free_pages(zone, page);
            }
        }

        private static void memset(byte* start, byte value, uint count)
        {
            for (uint i = 0; i < count; i++)
                start[i] = value;
        }

        /// <summary>
        ///  Set the properties of the combined page
        /// </summary>
        static void prepare_compound_pages(Page* page, byte order)
        {
            uint i;
            uint nr_pages = (1U << order);

            // The first page record combination page order value
            set_compound_order(page, order);
            __SetPageHead(page); // home page set head flag
            for (i = 1; i < nr_pages; i++)
            {
                Page* p = page + i;
                __SetPageTail(p); // The rest of the pages set the tail flag
                p->first_page = page;
            }
        }

        /// <summary>
        /// Split the combined page to get the page of the desired size
        /// </summary>
        static void expand(mem_zone* zone, Page* page,
                   byte low_order, byte high_order,
                   free_area* area)
        {
            uint size = (1U << high_order);
            while (high_order > low_order)
            {
                area--;
                high_order--;
                size >>= 1;
                list_head.list_add(&page[size].lru, &area->free_list);
                area->nr_free++;
                // set page order
                set_page_order_buddy(&page[size], high_order);
            }
        }

        static Page* __alloc_page(byte order,
                                  mem_zone* zone)
        {
            Page* page = null;
            free_area* area = null;
            byte current_order = 0;

            for (current_order = order;
                 current_order < BUDDY_MAX_ORDER; current_order++)
            {
                area = zone->free_area + current_order;
                if (list_head.list_empty(&area->free_list))
                {
                    continue;
                }
                // remove closest size page
                //page = list_entry(area->free_list.next, struct page, lru);
                page = (Page*)&((Page*)area->free_list.next)->lru;

                list_head.list_del(&page->lru);
                rmv_page_order_buddy(page);
                area->nr_free--;
                // expand to lower order
                expand(zone, page, order, current_order, area);
                // compound page
                if (order > 0)
                    prepare_compound_pages(page, order);
                else // single page
                    page->order = 0;
                return page;
            }
            return null;
        }

        private static Page* container_of(list_head* ptr)
        {
            return (Page*)((byte*)ptr - (uint)&((Page*)0)->lru);
        }

        public static Page* buddy_get_pages(mem_zone* zone,
                             byte order)
        {
            BUDDY_TRACE("buddy_get_pages, Order: {0:X8}", order);

            Page* page = null;

            if (order >= BUDDY_MAX_ORDER)
            {
                BUDDY_BUG("buddy_get_pages: order >= BUDDY_MAX_ORDER, {0}", order);
                return null;
            }
            //TODO: lock zone->lock
            page = __alloc_page(order, zone);
            //TODO: unlock zone->lock

            if (page == null)
                BUDDY_TRACE("Out of Memory?");

            return page;
        }

        ////////////////////////////////////////////////

        /// <summary>
        /// Destroy the combined page
        /// </summary>
        static bool destroy_compound_pages(Page* page, byte order)
        {
            int bad = 0;
            uint i;
            uint nr_pages = (1U << order);

            __ClearPageHead(page);
            for (i = 1; i < nr_pages; i++)
            {
                Page* p = page + i;
                if (!PageTail(p) || p->first_page != page)
                {
                    bad++;
                    BUDDY_BUG("destroy_compound_pages: error");
                }
                __ClearPageTail(p);
            }
            return bad != 0;
        }

        private static bool PageCompound(Page* page)
        {
            return (page->flags & ((1U << (byte)pageflags.PG_head) | (1U << (byte)pageflags.PG_tail))) != 0;
        }

        private static bool page_is_buddy(Page* page, byte order)
        {
            return (PageBuddy(page) && (page->order == order));
        }

        public static void buddy_free_pages(mem_zone* zone,
                       Page* page)
        {
            byte order = compound_order(page);
            uint buddy_idx = 0, combinded_idx = 0;
            uint page_idx = (uint)(page - zone->first_page);

            //TODO: lock zone->lock
            if (PageCompound(page))
                if (destroy_compound_pages(page, order))
                    BUDDY_BUG("buddy_free_pages: error");

            while (order < BUDDY_MAX_ORDER - 1)
            {
                Page* buddy;
                // find and delete buddy to combine
                buddy_idx = __find_buddy_index(page_idx, order);
                buddy = page + (buddy_idx - page_idx);
                if (!page_is_buddy(buddy, order))
                    break;
                list_head.list_del(&buddy->lru);
                zone->free_area[order].nr_free--;
                // remove buddy's flag and order
                rmv_page_order_buddy(buddy);
                // update page and page_idx after combined
                combinded_idx = __find_combined_index(page_idx, order);
                page = page + (combinded_idx - page_idx);
                page_idx = combinded_idx;
                order++;
            }
            set_page_order_buddy(page, order);
            list_head.list_add(&page->lru, &zone->free_area[order].free_list);
            zone->free_area[order].nr_free++;
            //TODO: unlock zone->lock
        }

        ////////////////////////////////////////////////

        public static void* page_to_virt(mem_zone* zone,
                   Page* page)
        {
            uint page_idx = 0;
            uint address = 0;

            page_idx = (uint)(page - zone->first_page);
            address = zone->start_addr + (page_idx * BUDDY_PAGE_SIZE);

            return (void*)address;
        }

        public static Page* virt_to_page(mem_zone* zone, void* ptr)
        {
            uint page_idx = 0;
            Page* page = null;
            uint address = (uint)ptr;

            if ((address < zone->start_addr) || (address > zone->end_addr))
            {
                //printf("start_addr=0x%lx, end_addr=0x%lx, address=0x%lx\n",
                //        zone->start_addr, zone->end_addr, address);
                BUDDY_BUG("virt_to_page: error");
                return null;
            }
            page_idx = (address - zone->start_addr) >> BUDDY_PAGE_SHIFT;

            page = zone->first_page + page_idx;
            return page;
        }

        public static uint buddy_num_free_page(mem_zone* zone)
        {
            byte i;
            uint ret;
            for (i = 0, ret = 0; i < BUDDY_MAX_ORDER; i++)
            {
                ret += zone->free_area[i].nr_free * (1U << i);
            }
            return ret;
        }
    }

}
