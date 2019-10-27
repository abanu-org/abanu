// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
//using pmeta = lonos.test.malloc4.malloc_meta*; //not possibe
using System.Runtime.InteropServices;
using malloc_meta = System.UInt32;
using size_t = System.UInt32;

#pragma warning disable

namespace Abanu.Test.malloc4
{

    public unsafe class Tester
    {

        private const byte PAGE_SHIFT = 12;

        alloc4 malloc;
        Random rnd = new Random();

        public void run()
        {
            malloc = new alloc4();

            var headsize = (alloc4.PAGE_SHIFT - 1) * IntPtr.Size;
            var addr = (byte*)Marshal.AllocHGlobal(headsize);

            for (var i = 0; i < headsize; i++)
                *(addr + i) = 0;

            malloc.List_heads = (malloc_meta**)addr;

            for (var i = 0; i < 10000; i++)
            {
                if (hash.Count > 3 && rnd.Next(2) == 0)
                {
                    RandomFree();
                }
                else
                {
                    Alloc((uint)rnd.Next(20) + 1);
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
            rnd.NextBytes(data);
            var ptr = (byte*)malloc.malloc(size);
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
                for (var i = 0; i < entry.Value.Data.Length; i++)
                {
                    if (ptr[i] != entry.Value.Data[i])
                        throw new Exception("Test failed");
                }
            }
        }

    }


}
