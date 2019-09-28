// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

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

namespace Lonos.CTypes
{

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public unsafe struct list_head
    {
        public list_head* next;
        public list_head* prev;

        public const uint LIST_POISON1 = 0x00100100;
        public const uint LIST_POISON2 = 0x00200200;

        public static void INIT_LIST_HEAD(list_head* list)
        {
            list->next = list;
            list->prev = list;
        }

        static void __list_add(list_head* New, list_head* prev, list_head* next)
        {
            next->prev = New;
            New->next = next;
            New->prev = prev;
            prev->next = New;
        }

        public static void list_add(list_head* New, list_head* head)
        {
            __list_add(New, head, head->next);
        }

        public static void list_add_tail(list_head* New, list_head* head)
        {
            __list_add(New, head->prev, head);
        }

        static void __list_del(list_head* prev, list_head* next)
        {
            next->prev = prev;
            prev->next = next;
        }

        static void __list_del_entry(list_head* entry)
        {
            __list_del(entry->prev, entry->next);
        }

        public static void list_del(list_head* entry)
        {
            __list_del(entry->prev, entry->next);
            entry->next = (list_head*)LIST_POISON1;
            entry->prev = (list_head*)LIST_POISON2;
        }

        public static void list_replace(list_head* old, list_head* New)
        {
            New->next = old->next;
            New->next->prev = New;
            New->prev = old->prev;
            New->prev->next = New;
        }

        public static bool list_empty(list_head* head)
        {
            return head->next == head;
        }

        #region Macros

        // #define container_of(ptr, type, member) \
        //   ((type *)((char *)(ptr)-(unsigned long)(&((type *)0)->member)))
        //
        // #define list_entry(ptr, type, member) \
        //   container_of(ptr, type, member)
        //
        // #define list_for_each(pos, head) \
        //   for (pos = (head)->next; pos != (head); pos = pos->next)

        #endregion

    }


}
