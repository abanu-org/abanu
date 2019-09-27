// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

//namespace Lonos.Test5
//{

//#pragma warning disable SA1300 // Element should begin with upper-case letter
//#pragma warning disable SA1307 // Accessible fields should begin with upper-case letter

//    public static unsafe class Alloc5
//    {
//        public const int BLOCK_SIZE = 4096;
//        public const int CACHE_L1_LINE_SIZE = 64;
//        public const int MAX_ORDER_LIMIT = 25;

//        public static int size_in_blocks(size_t size)
//        {
//            return (size / BLOCK_SIZE) + ((size % BLOCK_SIZE) != 0);
//        }

//        public static int size_of_blocks(int order)
//        {
//            return (1 << order) * BLOCK_SIZE;
//        }

//        public static int size_in_bytes(int block_count)
//        {
//            return BLOCK_SIZE * block_count;
//        }

//        public static int size_in_L1(int size)
//        {
//            //return size / CACHE_L1_LINE_SIZE + (size % CACHE_L1_LINE_SIZE != 0);
//            throw new NotImplementedException();
//        }

//        public static uint power_of_two(byte order)
//        {
//            return 1u << order;
//        }

//        private const uint NULL_INDEX = 0;
//        private const uint FIRST_ALLOC_INDEX = 1;

//        //private void* block_t;



//        public static uint get_block(uint block_index)
//        {
//            return (block_t)((char*)(mem_space) + BLOCK_SIZE * block_index);
//        }

//        //#define get_index(block_ptr) (block_index_t)((char*)block_ptr - (char*)mem_space)/BLOCK_SIZE;
//        //#define get_next_index(block_ptr) (block_index_t)(*(block_index_t*)block_ptr)
//        //#define set_next_index(cur_block_ptr, next_block_index) (block_index_t)(*(block_index_t*)cur_block_ptr) = next_block_index
//        //#define null_next_index(block_ptr)  (block_index_t)(*(block_index_t*)block_ptr) = NULL_INDEX

//        public static void* mem_space;

//        private static buddy_struct* buddy_ctrl_struct;

//        // Calculates maximum order of two found in num
//        private static uint calc_max_order(uint num)
//        {
//            int m = -1;
//            while (num > 0)
//            {
//                m++;
//                num >>= 1;
//            }
//            return (uint)m;
//        }

//        public static uint calc_block_order(size_t size)
//        {
//            byte order = 0;

//            if (size > BLOCK_SIZE)
//            {
//                order = 1;
//                while (power_of_two(order) < size_in_blocks(size))
//                    order++;
//            }

//            return order;
//        }

//        public static block_index_t calc_buddy_index(block_index_t block_index, byte order)
//        {
//            sbyte left, sign;
//            if (block_index % power_of_two(order) != 1 && order != 0)
//                return NULL_INDEX;

//            left = (block_index % power_of_two((byte)(order + 1))) == 1 ? (sbyte)1 : (sbyte)0;
//            sign = (sbyte)((-1 * (left == 0 ? 1 : 0)) + left);
//            return block_index + sign * power_of_two(order);
//        }

//        public static void put_first(block_index_t block_index, uint order)
//        {
//            block_index_t head_index;

//            head_index = buddy_ctrl_struct->free_heads[order];
//            set_next_index(get_block(block_index), head_index);
//            buddy_ctrl_struct->free_heads[order] = block_index;
//        }
//    }
//    internal unsafe struct buddy_struct
//    {
//        public void* alloc_space;
//        public block_count_t alloc_block_count;
//        public block_count_t free_block_count;

//        //public block_index_t free_heads[MAX_ORDER_LIMIT];
//        public block_index_t* free_heads; //TODO

//        public uint max_order;
//        public uint ctrl_offset;
//    }

//    public unsafe struct block_area
//    {
//        public void* addr;
//        public uint order;
//    }

//}
