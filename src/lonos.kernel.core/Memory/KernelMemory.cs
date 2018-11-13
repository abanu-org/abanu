using System;
using Mosa.Runtime;
using Mosa.Kernel.x86;
using Mosa.Runtime.Plug;
using Mosa.Runtime.x86;

namespace lonos.kernel.core
{

    public static class KernelMemory
    {
        static private uint heapStart = Address.GCInitialMemory;
        static private uint heapSize = 0x02000000;
        static private uint heapUsed = 0;

        [Plug("Mosa.Runtime.GC::AllocateMemory")]
        static unsafe private IntPtr _AllocateMemory(uint size)
        {
            return AllocateMemory(size);
        }

        private static uint addr;
        private static uint cnt;
        static public IntPtr AllocateMemory(uint size)
        {
            cnt++;
            Screen.Goto(1, cnt);
            Screen.Color = 4;
            Screen.Write("X");
            while (true) { Native.Nop(); };

            addr += size;
            return (IntPtr)(((uint)Address.GCInitialMemory) + addr);
        }
    }
   }
