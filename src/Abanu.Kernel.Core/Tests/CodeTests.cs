// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

namespace Abanu.Kernel.Core
{
    public static class CodeTests
    {

        public static void Run()
        {
            Ulongtest1();
            Ulongtest2();
            InlineTest();
        }

        private static void Ulongtest1()
        {
            uint mask = 0x00004000;
            uint v1 = 0x00000007;
            uint r1 = v1.SetBits(12, 52, mask, 12); //52 with uint makes no sense, but this doesn't matter in this case, the result just works as expected. It works correct with count<32, too, of course.
                                                    // r1 =  00004007
            ulong v2 = v1;
            ulong r2 = v2.SetBits(12, 52, mask, 12);
            uint r2Int = (uint)r2;
            // r2Int = 00000007. This is wrong. It should be the same as r1.

            KernelMessage.WriteLine("bla1: {0:X8}", r1);
            KernelMessage.WriteLine("bla2: {0:X8}", r2Int);
        }

        private static unsafe void InlineTest()
        {
            Addr addr = 0x1000;
            Addr addr2 = 0x1000u;
            Addr addr3 = addr + addr3;
        }

        private static unsafe void Ulongtest2()
        {
            ulong addr = 0x0000000019ad000;
            ulong data = 40004005;
            ulong result = data.SetBits(12, 52, addr, 12);

            var rAddr = (uint*)(void*)&result;
            var r1 = rAddr[0];
            var r2 = rAddr[1];

            KernelMessage.WriteLine("r1: {0:X8}", r1);
            KernelMessage.WriteLine("r2: {0:X8}", r2);
        }
    }

}
