// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
//using pmeta = lonos.test.malloc4.malloc_meta*; //not possibe
using System.Runtime.InteropServices;
using malloc_meta = System.UInt32;
using mem_zone = Lonos.Test.Alloc6.BuddyAllocatorImplementation.mem_zone;
using page = Lonos.Test.Alloc6.BuddyAllocatorImplementation.page;
using size_t = System.UInt32;

#pragma warning disable

namespace Lonos.Test.Alloc6
{

    public unsafe struct mem_block
    {
        public mem_zone zone;
        public page* pages;
    }

    public unsafe class Tester
    {

        Random rnd = new Random(2);

        static mem_block global_mem_block;
        public void run()
        {

            uint pages_size;
            uint start_addr;
            const uint _NPAGES = 8192;
            // init global memory block
            // all pages area
            pages_size = _NPAGES * (uint)sizeof(page);
            global_mem_block.pages = (page*)Marshal.AllocHGlobal((int)pages_size);
            start_addr = (uint)Marshal.AllocHGlobal((int)(_NPAGES * BuddyAllocatorImplementation.BUDDY_PAGE_SIZE));
            global_mem_block.zone.free_area = (BuddyAllocatorImplementation.free_area*)Marshal.AllocHGlobal(BuddyAllocatorImplementation.BUDDY_MAX_ORDER * (int)sizeof(BuddyAllocatorImplementation.free_area));

            fixed (mem_zone* zone = &global_mem_block.zone)
            {

                BuddyAllocatorImplementation.buddy_system_init(zone,
                                   global_mem_block.pages,
                                   start_addr,
                                   _NPAGES);
            }

            for (var i = 0; i < 10000; i++)
            {
                if (hash.Count > 3 && rnd.Next(2) == 0)
                {
                    RandomFree();
                }
                else
                {
                    var order = (byte)((uint)rnd.Next(4) + 1);
                    Alloc(order);
                }
            }
            System.Console.WriteLine(hash.Count);
        }

        private class AllocEnry
        {
            public byte[] Data;
        }

        private Dictionary<UIntPtr, AllocEnry> hash = new Dictionary<UIntPtr, AllocEnry>();

        public void Alloc(byte order)
        {
            var size = (1u << order) * 4096;
            fixed (mem_zone* zone = &global_mem_block.zone)
            {
                var data = new byte[size];
                rnd.NextBytes(data);
                var ptr = (byte*)BuddyAllocatorImplementation.page_to_virt(zone, BuddyAllocatorImplementation.buddy_get_pages(zone, order));
                if (ptr == null)
                {
                    var s = "";
                }
                for (var i = 0; i < size; i++)
                {
                    ptr[i] = data[i];
                }
                hash.Add((UIntPtr)ptr, new AllocEnry { Data = data });
                //ptr[10] = 0;
                Check();
            }
        }

        private void RandomFree()
        {
            fixed (mem_zone* zone = &global_mem_block.zone)
            {
                var keys = hash.Keys.ToList();
                var ptr = keys[rnd.Next(keys.Count - 1)];
                hash.Remove(ptr);
                var page = BuddyAllocatorImplementation.virt_to_page(zone, (void*)ptr);
                BuddyAllocatorImplementation.buddy_free_pages(zone, page);
            }
        }

        private void Check()
        {
            foreach (var entry in hash)
            {
                var ptr = (byte*)entry.Key;
                for (var i = 0; i < entry.Value.Data.Length; i++)
                {
                    if (ptr[i] != entry.Value.Data[i])
                        throw new Exception("Test failed");
                }
            }
        }

    }


}
