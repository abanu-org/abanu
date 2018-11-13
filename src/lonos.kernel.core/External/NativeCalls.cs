using System;
using Mosa.Runtime.x86;
using System.Runtime.InteropServices;

namespace lonos.kernel.core
{

    public static class NativeCalls
    {
        private static uint prog1Addr;
        private static uint prog2Addr;

        public static void Setup()
        {
            prog1Addr = KernelElf.Native.GetPhysAddrOfSymbol("proc1");
            prog2Addr = KernelElf.Native.GetPhysAddrOfSymbol("proc2");
        }

        public static void proc1()
        {
            Native.Call(prog1Addr);
        }

        public static void proc2()
        {
            Native.Call(prog2Addr);
        }

    }

}
