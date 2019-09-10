using System;
using Mosa.Runtime;
using Mosa.Kernel.x86;
using Mosa.Runtime.Plug;
using Mosa.Runtime.x86;
using Lonos.Kernel.Core.Boot;
using Lonos.Kernel.Core;

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
        static unsafe private IntPtr AllocateMemoryPlug(uint size)
        {
            return AllocateMemory(size);
        }

        private static uint nextAddr;
        static public IntPtr AllocateMemory(uint size)
        {
            var retAddr = nextAddr;
            nextAddr += size;

            return (IntPtr)(((uint)Address.GCInitialMemory) + retAddr);
        }

        static Addr PageStartAddr;
        public static BootInfoMemory AllocateMemoryMap(USize size, BootInfoMemoryType type)
        {
            var map = new BootInfoMemory
            {
                Start = PageStartAddr,
                Size = size,
                Type = type
            };
            PageStartAddr += size;

            KernelMessage.WriteLine("Allocated MemoryMap of Type {0} at {1:X8} with Size {2:X8}", (uint)type, map.Start, map.Size);

            return map;
        }

    }
}
