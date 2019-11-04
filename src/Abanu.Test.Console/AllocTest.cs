// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

using Abanu.Kernel.Core;

namespace Abanu.Test.Console
{

    public unsafe class AllocTest
    {
        public AllocTest()
        {
        }

        private MyAlloc malloc;
        private Random rnd = new Random();

        public void Run()
        {
            malloc = new MyAlloc();
            malloc.Init();
            for (var i = 0; i < 10000; i++)
            {
                if (hash.Count > 3 && rnd.Next(2) == 0)
                {
                    RandomFree();
                }
                else
                {
                    Alloc((uint)rnd.Next(20));
                }
            }
            System.Console.WriteLine(hash.Count);
        }

        private class AllocEnry
        {
            public byte[] Data;
        }

        private Dictionary<UIntPtr, AllocEnry> hash = new Dictionary<UIntPtr, AllocEnry>();

        public void Alloc(uint size)
        {
            var data = new byte[size];
            var bucket = BinaryBuddyAllocator_TestImplementation.Bucket_for_request(size + 8);
            rnd.NextBytes(data);
            var page = malloc.Malloc(bucket);
            for (var i = 0; i < size; i++)
            {
                ((byte*)page->ptr)[i] = data[i];
            }
            hash.Add((UIntPtr)page, new AllocEnry { Data = data });
            //ptr[10] = 0;
            Check();
        }

        private void RandomFree()
        {
            var keys = hash.Keys.ToList();
            var ptr = keys[rnd.Next(keys.Count - 1)];
            hash.Remove(ptr);
            malloc.Free((BinaryBuddyAllocator_TestImplementation.Page*)ptr);
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

    public unsafe class MyAlloc : BinaryBuddyAllocator_TestImplementation
    {

        private void* addr;

        protected override bool Parent_is_split(uint index)
        {
            index = (index - 1) / 2;
            return firstPage[index].parent_is_split;
        }

        /*
         * Given the index of a node, this flips the "is split" flag of the parent.
         */
        protected override void Flip_parent_is_split(uint index)
        {
            index = (index - 1) / 2;
            firstPage[index].parent_is_split = !firstPage[index].parent_is_split;
        }

        public MyAlloc()
        {
            addr = (void*)Marshal.AllocHGlobal(128 * 1024 * 1024);
            buckets = (List_t*)Marshal.AllocHGlobal(sizeof(List_t) * BUCKET_COUNT);
            var nodeIsSplitSize = sizeof(Page) * 32768;
            firstPage = (Page*)Marshal.AllocHGlobal(nodeIsSplitSize);
            var data = (byte*)Marshal.AllocHGlobal(32768 * 4096);

            for (var i = 0; i < 32768; i++)
            {
                firstPage[i].ptr = data + (i * 4096);
            }
        }

        protected override unsafe bool Brk(void* addr)
        {
            return true;
        }

        protected override unsafe void* Sbrk(uint size)
        {
            return addr;
        }

    }

}
