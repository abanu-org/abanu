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
#pragma warning disable SA1307 // Accessible fields should begin with upper-case letter
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Lonos.CTypes
{

    /// <summary>
    /// Simple doubly linked list implementation.
    /// </summary>
    /// <remarks>
    /// Some of the internal functions("__xxx") are useful when manipulating whole lists rather than single entries, as
    /// sometimes we already know the next/prev entries and we can generate better code by using them directly rather than
    /// using the generic single-entry routines.
    /// </remarks>
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

        /// <summary>
        /// Insert a new entry between two known consecutive entries.
        /// </summary>
        /// <remarks>
        /// This is only for internal list manipulation where we know the prev/next entries already!
        /// </remarks>
        private static void __list_add(list_head* New, list_head* prev, list_head* next)
        {
            next->prev = New;
            New->next = next;
            New->prev = prev;
            prev->next = New;
        }

        /// <summary>
        /// Add a new entry
        /// </summary>
        /// <param name="New">new entry to be added</param>
        /// <param name="head">list head to add it after</param>
        /// <remarks>
        /// Insert a new entry after the specified head. This is good for implementing stacks.
        /// </remarks>
        public static void list_add(list_head* New, list_head* head)
        {
            __list_add(New, head, head->next);
        }

        /// <summary>
        /// Add a new entry
        /// </summary>
        /// <param name="New">new entry to be added</param>
        /// <param name="head">list head to add it before</param>
        /// <remarks>
        /// Insert a new entry before the specified head. This is useful for implementing queues.
        /// </remarks>
        public static void list_add_tail(list_head* New, list_head* head)
        {
            __list_add(New, head->prev, head);
        }

        /// <summary>
        /// Delete a list entry by making the prev/next entries point to each other.
        /// </summary>
        /// <remarks>
        /// This is only for internal list manipulation where we know the prev/next entries already!
        /// </remarks>
        private static void __list_del(list_head* prev, list_head* next)
        {
            next->prev = prev;
            prev->next = next;
        }

        private static void __list_del_entry(list_head* entry)
        {
            __list_del(entry->prev, entry->next);
        }

        /// <summary>
        /// Deletes entry from list.
        /// </summary>
        /// <param name="entry">The element to delete from the list.</param>
        /// <remarks>
        /// Note: <see cref="list_empty(list_head*)"/> on entry does not return true after this, the entry is in an undefined state.
        /// </remarks>
        public static void list_del(list_head* entry)
        {
            __list_del(entry->prev, entry->next);
            entry->next = (list_head*)LIST_POISON1;
            entry->prev = (list_head*)LIST_POISON2;
        }

        /// <summary>
        /// Replace old entry by new one
        /// </summary>
        /// <param name="old">The element to be replaced</param>
        /// <param name="New">The new element to insert</param>
        public static void list_replace(list_head* old, list_head* New)
        {
            New->next = old->next;
            New->next->prev = New;
            New->prev = old->prev;
            New->prev->next = New;
        }

        /// <summary>
        /// tests whether a list is empty
        /// </summary>
        /// <param name="head">The list to test</param>
        public static bool list_empty(list_head* head)
        {
            return head->next == head;
        }

        /// <summary>
        /// Tests whether a list has just one entry.
        /// </summary>
        /// <param name="head">The list to test</param>
        public static bool list_is_singular(list_head* head)
        {
            return !list_empty(head) && (head->next == head->prev);
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
