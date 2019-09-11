// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using Lonos.Kernel.Core;
using Lonos.Kernel.Core.Boot;
using Mosa.Runtime;
using Mosa.Runtime.Plug;
using Mosa.Runtime.x86;

namespace Lonos.Kernel.Loader
{

    public static class BootMemory
    {

        public static void Setup()
        {
            PageStartAddr = Address.InitialDynamicPage;
            //PageStartAddr = Address.GCInitialMemory;
        }

        [Plug("Mosa.Runtime.GC::AllocateMemory")]
        private static unsafe IntPtr AllocateMemoryPlug(uint size)
        {
            return AllocateMemory(size);
        }

        private static uint nextAddr;

        public static IntPtr AllocateMemory(uint size)
        {
            var retAddr = nextAddr;
            nextAddr += size;

            return (IntPtr)(((uint)Address.GCInitialMemory) + retAddr);
        }

        private static Addr PageStartAddr;

        public static BootInfoMemory AllocateMemoryMap(USize size, BootInfoMemoryType type)
        {
            var map = new BootInfoMemory
            {
                Start = PageStartAddr,
                Size = size,
                Type = type,
            };
            PageStartAddr += size;

            KernelMessage.WriteLine("Allocated MemoryMap of Type {0} at {1:X8} with Size {2:X8}", (uint)type, map.Start, map.Size);

            return map;
        }

    }
}
