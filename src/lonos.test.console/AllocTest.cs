using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;


namespace lonos.test.console
{

    public unsafe class AllocTest
    {
        public AllocTest()
        {
        }

        MyAlloc malloc;
        Random rnd = new Random();
        public void run()
        {
            malloc = new MyAlloc();
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
            Console.WriteLine(hash.Count);
        }

        private class AllocEnry
        {
            public byte[] data;
        }

        private Dictionary<UIntPtr, AllocEnry> hash = new Dictionary<UIntPtr, AllocEnry>();

        public void Alloc(uint size)
        {
            var data = new byte[size];
            var bucket = BinaryBuddyAllocator_TestImplementation.bucket_for_request(size+8);
            rnd.NextBytes(data);
            var ptr = (byte*)malloc.malloc(bucket);
            for (var i = 0; i < size; i++)
            {
                ptr[i] = data[i];
            }
            hash.Add((UIntPtr)ptr, new AllocEnry { data = data });
            //ptr[10] = 0;
            Check();
        }

        private void RandomFree()
        {
            var keys = hash.Keys.ToList();
            var ptr = keys[rnd.Next(keys.Count - 1)];
            hash.Remove(ptr);
            malloc.free((void*)ptr);
        }

        private void Check()
        {
            foreach (var entry in hash)
            {
                var ptr = (byte*)entry.Key;
                for (var i = 0; i < entry.Value.data.Length; i++)
                {
                    if (ptr[i] != entry.Value.data[i])
                        throw new Exception("Test failed");
                }
            }
        }

    }

    public unsafe class MyAlloc : BinaryBuddyAllocator_TestImplementation
    {

        private void* addr;

        protected page* node_is_split;

        protected override bool parent_is_split(uint index)
        {
            index = (index - 1) / 2;
            return ((node_is_split[index / 8].parent_is_split >> (byte)(index % 8)) & 1) != 0;
        }

        /*
         * Given the index of a node, this flips the "is split" flag of the parent.
         */
        protected override void flip_parent_is_split(uint index)
        {
            index = (index - 1) / 2;
            node_is_split[index / 8].parent_is_split ^= (byte)(1 << (byte)(index % 8));
        }

        public MyAlloc()
        {
            addr = (void*)Marshal.AllocHGlobal(128 * 1024 * 1024);
            buckets = (list_t*)Marshal.AllocHGlobal(sizeof(list_t) * BUCKET_COUNT);
            var nodeIsSplitSize = sizeof(page) * ((1 << (BUCKET_COUNT - 1)) / 8);
            node_is_split = (page*)Marshal.AllocHGlobal(nodeIsSplitSize);
        }

        protected override unsafe bool brk(void* addr)
        {
            return true;
        }

        protected override unsafe void* sbrk(uint size)
        {
            return addr;
        }

    }

}
