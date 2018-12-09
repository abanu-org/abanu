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
                if (hash.Count>3 && rnd.Next(2) == 0)
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
            rnd.NextBytes(data);
            var ptr = (byte*)malloc.malloc(size);
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

        public MyAlloc()
        {
            addr = (void*)Marshal.AllocHGlobal(128 * 1024 * 1024);
            buckets = (list_t*)Marshal.AllocHGlobal(sizeof(list_t) * BUCKET_COUNT);
            node_is_split = (byte*)Marshal.AllocHGlobal(sizeof(void*) * ((1 << (BUCKET_COUNT - 1)) / 8));
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
