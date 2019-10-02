// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
        /// Delete from one list and add as another's head
        /// </summary>
        /// <param name="list">the entry to move</param>
        /// <param name="head">the head that will precede our entry</param>
        public static void list_move(list_head* list, list_head* head)
        {
            __list_del_entry(list);
            list_add(list, head);
        }

        /// <summary>
        /// Delete from one list and add as another's tail
        /// </summary>
        /// <param name="list">The entry to move</param>
        /// <param name="head">The head that will follow our entry</param>
        public static void list_move_tail(list_head* list, list_head* head)
        {
            __list_del_entry(list);
            list_add_tail(list, head);
        }

        /// <summary>
        /// Move a subsection of a list to its tail
        /// </summary>
        /// <param name="head"> the head that will follow our entry</param>
        /// <param name="first">First entry to move</param>
        /// <param name="last">Last entry to move, can be the same as first</param>
        /// <remarks>
        /// Move all entries between <paramref name="first"/> and including <paramref name="last"/> before <paramref name="head"/>.
        /// All three entries must belong to the same linked list.
        /// </remarks>
        public static void list_bulk_move_tail(list_head* head, list_head* first, list_head* last)
        {
            first->prev->next = last->next;
            last->next->prev = first->prev;

            head->prev->next = first;
            first->prev = head->prev;

            last->next = head;
            head->prev = last;
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

        /// <summary>
        /// Tests whether <paramref name="list"/> is the first entry in list <paramref name="head"/>
        /// </summary>
        /// <param name="list">The entry to test</param>
        /// <param name="head">The head of the list</param>
        public static bool list_is_first(list_head* list, list_head* head)
        {
            return list->prev == head;
        }

        /// <summary>
        /// Tests whether <paramref name="list"/> is the last entry in list <paramref name="head"/>
        /// </summary>
        /// <param name="list">The entry to test</param>
        /// <param name="head">The head of the list</param>
        public static bool list_is_last(list_head* list, list_head* head)
        {
            return list->next == head;
        }

        /// <summary>
        /// Deletes entry from list and reinitialize it.
        /// </summary>
        /// <param name="entry">The element to delete from the list.</param>
        public static void list_del_init(list_head* entry)
        {
            __list_del_entry(entry);
            INIT_LIST_HEAD(entry);
        }

        /// <summary>
        /// Rotate the list to the left
        /// </summary>
        /// <param name="head">The head of the list</param>
        public static void list_rotate_left(list_head* head)
        {
            list_head* first;
            if (!list_empty(head))
            {
                first = head->next;
                list_move_tail(first, head);
            }
        }

        /// <summary>
        /// Rotate list to specific item.
        /// </summary>
        /// <param name="list">The desired new front of the list.</param>
        /// <param name="head">The head of the list.</param>
        /// <remarks>
        /// Deletes the list head from the list denoted by @head and
        /// places it as the tail of <paramref name="list"/>, this effectively rotates the
        /// list so that <paramref name="list"/> is at the front.
        /// </remarks>
        public static void list_rotate_to_front(list_head* list, list_head* head)
        {
            list_move_tail(head, list);
        }

        private static void __list_cut_position(list_head* list, list_head* head, list_head* entry)
        {
            list_head* new_first = entry->next;
            list->next = head->next;
            list->next->prev = list;
            list->prev = entry;
            entry->next = list;
            head->next = new_first;
            new_first->prev = head;
        }

        /// <summary>
        /// Cut a list into two
        /// </summary>
        /// <param name="list">A new list to add all removed entries</param>
        /// <param name="head">A list with entries</param>
        /// <param name="entry">An entry within head, could be the head itself and if so we won't cut the list</param>
        /// <remarks>
        /// This helper moves the initial part of <paramref name="head"/>, up to and including <paramref name="entry"/>,
        /// from <paramref name="head"/> to <paramref name="list"/>.You should pass on <paramref name="entry"/> an element
        /// you know is on <paramref name="head"/>.<paramref name="list"/> should be an empty list or a list you do not care about
        /// losing its data.
        /// </remarks>
        public static void list_cut_position(list_head* list, list_head* head, list_head* entry)
        {
            if (list_empty(head))
                return;
            if (list_is_singular(head) &&
                (head->next != entry && head != entry))
                return;
            if (entry == head)
                INIT_LIST_HEAD(list);
            else
                __list_cut_position(list, head, entry);
        }

        /// <summary>
        /// Cut a list into two, before given entry
        /// </summary>
        /// <param name="list">A new list to add all removed entries</param>
        /// <param name="head">A list with entries</param>
        /// <param name="entry">An entry within head, could be the head itself</param>
        /// <remarks>
        /// This helper moves the initial part of <paramref name="head"/>, up to but
        /// excluding <paramref name="entry"/>, from <paramref name="head"/> to <paramref name="list"/>. You should pass
        /// in <paramref name="entry"/> an element you know is on <paramref name="head"/>.<paramref name="list"/> should
        /// be an empty list or a list you do not care about losing its data.
        /// If <paramref name="entry"/> == <paramref name="head"/>, all entries on <paramref name="head"/> are moved to <paramref name="list"/>.
        /// </remarks>
        public static void list_cut_before(list_head* list, list_head* head, list_head* entry)
        {
            if (head->next == entry)
            {
                INIT_LIST_HEAD(list);
                return;
            }
            list->next = head->next;
            list->next->prev = list;
            list->prev = entry->prev;
            list->prev->next = list;
            head->next = entry;
            entry->prev = head;
        }

        private static void __list_splice(list_head* list, list_head* prev, list_head* next)
        {

            list_head* first = list->next;
            list_head* last = list->prev;

            first->prev = prev;
            prev->next = first;

            last->next = next;
            next->prev = last;
        }

        /// <summary>
        /// Join two lists, this is designed for stacks
        /// </summary>
        /// <param name="list">The new list to add.</param>
        /// <param name="head">The place to add it in the first list.</param>
        public static void list_splice(list_head* list, list_head* head)
        {
            if (!list_empty(list))
                __list_splice(list, head, head->next);
        }

        /// <summary>
        /// Join two lists, each list being a queue
        /// </summary>
        /// <param name="list">The new list to add.</param>
        /// <param name="head">The place to add it in the first list.</param>
        public static void list_splice_tail(list_head* list, list_head* head)
        {
            if (!list_empty(list))
                __list_splice(list, head->prev, head);
        }

        /// <summary>
        /// Join two lists and reinitialize the emptied list.
        /// </summary>
        /// <param name="list">The new list to add.</param>
        /// <param name="head">The place to add it in the first list.</param>
        /// <remarks>
        /// The list at <paramref name="list"/> is reinitialized
        /// </remarks>
        public static void list_splice_init(list_head* list, list_head* head)
        {
            if (!list_empty(list))
            {
                __list_splice(list, head, head->next);
                INIT_LIST_HEAD(list);
            }
        }

        /// <summary>
        /// Join two lists and reinitialize the emptied list
        /// </summary>
        /// <param name="list">The new list to add.</param>
        /// <param name="head">The place to add it in the first list.</param>
        /// <remarks>
        /// Each of the lists is a queue.
        /// The list at <paramref name="list"/> is reinitialized
        /// </remarks>
        public static void list_splice_tail_init(list_head* list, list_head* head)
        {
            if (!list_empty(list))
            {
                __list_splice(list, head->prev, head);
                INIT_LIST_HEAD(list);
            }
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

        /// <summary>
        /// Returns the size of given list
        /// </summary>
        public static uint list_count(list_head* head)
        {
            list_head* temp = head;
            uint result = 0;

            if (head != null)
            {
                do
                {
                    temp = temp->next;
                    result++;
                }
                while (temp != head);
            }

            return result;
        }

    }

}
