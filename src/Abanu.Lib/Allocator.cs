// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;
//using pmeta = abanu.test.malloc4.malloc_meta*; //not possibe
using System.Runtime.InteropServices;

#pragma warning disable SA1649 // File name should match first type name
#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
#pragma warning disable SA1303 // Const field names should begin with upper-case letter

#if BITS_64
using malloc_meta = System.UInt64;
using size_t = System.UInt64;
#else
using malloc_meta = System.UInt32;
using size_t = System.UInt32;
#endif

// Do not modify this Allocator very much
// Future: Assign mmap-method dynamically.

namespace Abanu
{

    public unsafe struct malloc_list
    {
        public malloc_meta* Next;
        public malloc_meta* Prev;
    }

    public unsafe struct malloc_data
    {
        public malloc_list Free;

        //char user[0];
        public byte* user // here begins the user data
        {
            get
            {
                fixed (malloc_data* ptr = &this)
                {
                    var bptr = (byte*)ptr;
                    return bptr;
                }
            }
        }

    }

    public unsafe struct malloc_meta
    {
        public size_t Size;
        public malloc_data Data;
    }

    /// <summary>
    /// Simple Heap Memory Allocator
    /// </summary>
    public unsafe abstract class Allocator
    {

#if BITS_64
        private const byte sizofSize_t = 8;
#else
        private const byte sizofSize_t = 4;
#endif

        public const size_t headSize = (PAGE_SHIFT - 1) * sizofSize_t;

        public const byte PAGE_SHIFT = 12;
        private const size_t PAGE_SIZE = 1 << PAGE_SHIFT;
        private const byte MIN_SHIFT = 5;
        private const size_t MIN_SIZE = 1 << MIN_SHIFT;
        private const uint META_SIZE = sizofSize_t;

        private const size_t MAX_SHIFT_BIT = (size_t)1 << ((sizofSize_t * 8) - 1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static malloc_meta* MIN(malloc_meta* A, malloc_meta* B)
        {
            return A > B ? B : A;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static size_t MIN(size_t A, size_t B)
        {
            return A > B ? B : A;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SET_INUSE(malloc_meta* P)
        {
            P->Size &= ~MAX_SHIFT_BIT;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static size_t SET_FREE(malloc_meta* P)
        {
            return P->Size |= MAX_SHIFT_BIT;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static size_t IS_FREE(malloc_meta* P)
        {
            return P->Size & MAX_SHIFT_BIT;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static size_t GET_SIZE(malloc_meta* P)
        {
            return P->Size & ~MAX_SHIFT_BIT;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static size_t SET_SIZE(malloc_meta* P, size_t NSIZE)
        {
            return P->Size = IS_FREE(P) | NSIZE;
        }

        private const size_t MALLOC_MAX_SIZE = ~MAX_SHIFT_BIT;

        //malloc_meta* list_heads[PAGE_SHIFT - 1];
        public malloc_meta** List_heads;

        protected abstract void malloc_abort(string msg);

        private static size_t order(size_t l)
        {
            size_t ord = 0;
            size_t current = 1;

            if (l == 0)
                return 0;

            while ((l & current) == 0)
            {
                current <<= 1;
                ord++;
            }

            return ord;
        }

        private static byte round_up_binary(size_t n)
        {
            byte res = 0;
            size_t cur = 1;

            while (n > (cur << res))
                res++;

            return res;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static size_t size_to_page_number(size_t size)
        {
            return ((size - 1) / PAGE_SIZE) + 1;
        }

        private void add_to_free_list(malloc_meta* new_)
        {
            malloc_meta** list_head = &List_heads[order(GET_SIZE(new_)) - MIN_SHIFT];

            SET_FREE(new_);
            new_->Data.Free.Next = *list_head;
            new_->Data.Free.Prev = null;
            if ((*list_head) != null)
                (*list_head)->Data.Free.Prev = new_;
            *list_head = new_;
        }

        private void remove_from_free_list(malloc_meta* to_del)
        {

            malloc_meta** list_head = &List_heads[order(GET_SIZE(to_del)) - MIN_SHIFT];

            SET_INUSE(to_del);
            if (*list_head == to_del)
                *list_head = to_del->Data.Free.Next;
            if (to_del->Data.Free.Next != null)
                to_del->Data.Free.Next->Data.Free.Prev = to_del->Data.Free.Prev;
            if (to_del->Data.Free.Prev != null)
                to_del->Data.Free.Prev->Data.Free.Next = to_del->Data.Free.Next;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static malloc_meta* create_meta(void* p, size_t l)
        {
            malloc_meta* meta = (malloc_meta*)p;

            SET_SIZE(meta, l);
            SET_INUSE(meta);
            return meta;
        }

        private void create_buddy(malloc_meta* b)
        {
            size_t buddy = (size_t)b;
            size_t size = GET_SIZE(b);

            size >>= 1;
            SET_SIZE(b, size);
            buddy += size;
            malloc_meta* buddy_meta = (malloc_meta*)buddy;
            SET_SIZE(buddy_meta, size);
            add_to_free_list(buddy_meta);
        }

        private static malloc_meta* find_buddy(malloc_meta* s)
        {
            size_t addr = (size_t)s;
            addr ^= GET_SIZE(s);

            return (malloc_meta*)addr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static malloc_meta* get_meta(void* ptr)
        {
            byte* cptr = (byte*)ptr;
            return (malloc_meta*)(cptr - META_SIZE);
        }

        private malloc_meta* split(malloc_meta* b, size_t s)
        {
            if (GET_SIZE(b) == s || GET_SIZE(b) == MIN_SIZE)
                return b;

            create_buddy(b);
            return split(b, s);
        }

        private malloc_meta* fusion(malloc_meta* b, size_t s)
        {
            if (GET_SIZE(b) >= PAGE_SIZE || GET_SIZE(b) == s)
                return b;

            malloc_meta* buddy = find_buddy(b);
            if (IS_FREE(buddy) == 0 || GET_SIZE(buddy) != GET_SIZE(b))
                return b;

            remove_from_free_list(buddy);
            malloc_meta* left_buddy = MIN(b, buddy);
            SET_SIZE(left_buddy, GET_SIZE(left_buddy) << 1);
            return fusion(left_buddy, s);
        }

        private void* large_malloc(size_t size)
        {
            void* ptr = mmap(0, PROT_READ | PROT_WRITE, 1 + ((size - 1) / PAGE_SIZE));
            if (ptr == MAP_FAILED)
            {
                malloc_abort("[MALLOC] MMAP ERROR\n");
                return null;
            }
            malloc_meta* meta = create_meta(ptr, size);

            return meta->Data.user;
        }

        private malloc_meta* find_free_block(size_t size)
        {
            size_t current = order(size) - MIN_SHIFT;
            size_t max = PAGE_SHIFT - MIN_SHIFT;

            for (; current < max; current++)
            {
                malloc_meta* block = List_heads[current];
                if (block != null)
                {
                    remove_from_free_list(block);
                    return block;
                }
            }

            return null;
        }

        private static size_t internal_size(size_t size)
        {
            size += META_SIZE;

            if (size >= PAGE_SIZE)
                return size_to_page_number(size) * PAGE_SIZE;

            if (size < MIN_SIZE)
                size = MIN_SIZE;
            size = 1U << round_up_binary(size);
            return size;
        }

        private void* buddy_malloc(size_t size)
        {
            lock (this)
            {
                malloc_meta* block = find_free_block(size);
                if (block != null)
                {
                    block = split(block, size);
                    return block->Data.user;
                }

                void* ptr = mmap(0, PROT_READ | PROT_WRITE, 1);
                if (ptr == MAP_FAILED)
                {
                    malloc_abort("[MALLOC] MMAP ERROR\n");
                    return null;
                }

                malloc_meta* meta = create_meta(ptr, PAGE_SIZE);
                meta = split(meta, size);
                return meta->Data.user;
            }
        }

        public void free(Addr ptr)
        {
            if (ptr == null)
                return;

            malloc_meta* meta = get_meta(ptr);
            lock (this)
            {

                meta = fusion(meta, PAGE_SIZE);

                if (GET_SIZE(meta) >= PAGE_SIZE)
                {
                    if (munmap(meta) != 0)
                    {
                        malloc_abort("[FREE] MUNMAP ERROR\n");
                    }
                    return;
                }

                add_to_free_list(meta);
            }
        }

        public Addr malloc(size_t size)
        {
            if (size == 0)
                return null;

            void* ptr = null;
            size = internal_size(size);

            if (size > MALLOC_MAX_SIZE)
                return null;
            if (size > PAGE_SIZE)
            {
                ptr = large_malloc(size);
                return ptr;
            }

            ptr = buddy_malloc(size);
            return ptr;
        }

        private void* realloc(void* ptr, size_t size)
        {
            if (ptr == null)
                return malloc(size);
            if (size == 0)
            {
                free(ptr);
                return null;
            }

            size_t user_size = size;
            size = internal_size(size);
            malloc_meta* meta = get_meta(ptr);

            if (size > MALLOC_MAX_SIZE)
                return null;

            if (size <= PAGE_SIZE)
            {
                if (size == GET_SIZE(meta))
                {
                    return ptr;
                }
                else if (size > GET_SIZE(meta))
                {
                    size_t previous_size = GET_SIZE(meta) - META_SIZE;

                    lock (this)
                    {
                        meta = fusion(meta, size);
                        if (GET_SIZE(meta) == size)
                        {
                            SET_INUSE(meta);

                            if (ptr != meta->Data.user)
                                memcpy(meta->Data.user, ptr, previous_size);
                            return meta->Data.user;
                        }
                    }
                }
                else
                {
                    lock (this)
                    {
                        meta = split(meta, size);
                    }
                    return meta->Data.user;
                }
            }

            size = user_size;
            void* new_ptr = malloc(size);
            if (new_ptr == null)
                return null;
            malloc_meta* mmeta = get_meta(ptr);
            memcpy(new_ptr, ptr, MIN(size, GET_SIZE(mmeta) - META_SIZE));
            free(ptr);

            return new_ptr;
        }

        private void* calloc(size_t nmenb, size_t size)
        {
            size_t mem_size = nmenb * size;
            void* ptr = null;

            if (mem_size == 0)
                return null;

            ptr = malloc(mem_size);
            if (ptr == null)
                return null;
            memset(ptr, 0, mem_size);
            return ptr;
        }

        private static void memcpy(void* addr1, void* addr2, size_t size)
        {
        }

        private static void memset(void* addr1, byte value, size_t size)
        {
        }

        protected abstract void* mmap(uint unknown, uint flags, size_t pages);

        protected abstract uint munmap(void* addr);

        private const uint PROT_READ = 0; //unknown
        private const uint PROT_WRITE = 0; //unknown
        private static void* MAP_FAILED = (void*)1; //unknown

    }
}
