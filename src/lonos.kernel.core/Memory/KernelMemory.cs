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

        private static uint nextAddr;
        private static uint cnt;
        static public IntPtr AllocateMemory(uint size)
        {
            cnt++;

            var retAddr = nextAddr;
            nextAddr += size;

            var col = Screen.column;
            var row = Screen.row;
            Screen.column = 0;
            Screen.Goto(0, 35);
            Screen.Write("AllocCount: ");
            Screen.Write(cnt);
            Screen.Goto(1, 35);
            Screen.Write("AllocSize:  ");
            Screen.Write(nextAddr);
            Screen.row = row;
            Screen.column = col;

            return (IntPtr)(((uint)Address.GCInitialMemory) + retAddr);
        }

        public static void DumpToConsoleLine(uint addr, uint length)
        {
            DumpToConsole(addr, length);
            KernelMessage.Write('\n');
        }

        public static void DumpToConsole(uint addr, uint length)
        {
            var sb = new StringBuffer();
            sb.Append("{0:X}+{1:D} ", addr, length);
            KernelMessage.Write(sb);
            sb.Clear();

            for (uint a = addr; a < addr + length; a++)
            {
                sb.Clear();

                if (a != addr)
                    sb.Append(" ");
                var m = Native.Get8(a);
                sb.Append(m, 16, 2);
                KernelMessage.Write(sb);
            }
        }

    }
}
