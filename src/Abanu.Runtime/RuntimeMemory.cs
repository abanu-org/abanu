// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System;
using Mosa.Runtime.Plug;

namespace Abanu.Runtime
{
    public static class RuntimeMemory
    {
        [Plug("Mosa.Runtime.GC::AllocateMemory")]
        private static unsafe IntPtr AllocateMemoryPlug(uint size)
        {
            return (IntPtr)AllocateCleared(size);
        }

        internal static void SetupEarlyStartup()
        {
            initialMemoryNextAddr = SysCalls.RequestMemory(3 * 1024 * 1024);
        }

        internal static unsafe void SetupAllocator()
        {
            Allocator = new RuntimeAllocator();
            var ptr = (byte*)SysCalls.RequestMemory(KMath.AlignValueCeil(Allocator.headSize, 4096));
            for (var i = 0; i < Allocator.headSize; i++)
                *(ptr + i) = 0; // TODO: Optimize
            Allocator.List_heads = (malloc_meta**)ptr;
            AllocatorInitialized = true;
        }

        private static uint initialMemoryNextAddr;
        private static Allocator Allocator;
        private static bool AllocatorInitialized = false;

        public static unsafe uint Allocate(int size)
        {
            return Allocate((uint)size);
        }

        public static unsafe uint Allocate(uint size)
        {
            if (AllocatorInitialized)
                return (uint)Allocator.malloc(size);

            var retAddr = initialMemoryNextAddr;
            initialMemoryNextAddr += size;
            return retAddr;
        }

        public static unsafe uint AllocateCleared(int size)
        {
            return AllocateCleared((uint)size);
        }

        public static unsafe uint AllocateCleared(long size)
        {
            return AllocateCleared((uint)size);
        }

        public static unsafe uint AllocateCleared(uint size)
        {
            var ptr = Allocate(size);
            Clear(ptr, size);
            return ptr;
        }

        public static unsafe void Clear(Addr addr, USize size)
        {
            Set(addr, 0, size);
        }

        public static unsafe void Set(Addr addr, byte value, USize size)
        {
            var bytePtr = (byte*)addr;
            var len = (uint)size;
            for (var i = 0; i < len; i++)
                bytePtr[i] = value;
        }

        /// <summary>
        /// free previously allocated memory
        /// </summary>
        public static unsafe void Free(Addr address)
        {
            if (!AllocatorInitialized)
                return;

            Allocator.free(address);
        }

        /// <summary>
        /// free previously allocated memory
        /// </summary>
        public static unsafe void Free(IntPtr address)
        {
            if (!AllocatorInitialized)
                return;

            Allocator.free((void*)address);
        }

        public static unsafe void FreeObject(object obj)
        {
            if (!AllocatorInitialized)
                return;

            var ptr = Mosa.Runtime.Intrinsic.GetObjectAddress(obj);
            Allocator.free((void*)ptr);
        }

    }

}
