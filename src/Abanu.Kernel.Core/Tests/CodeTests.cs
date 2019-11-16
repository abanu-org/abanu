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

            StructRef1();
            StructRef2();
            StructRef3();
            SizeOf();
        }

        private static unsafe void StructRef1()
        {
            var reg = new MemoryRegion(0x33, 0x44);
            var regPtr = &reg;
            ref MemoryRegion m = ref StructRef1_test(regPtr);
            KernelMessage.WriteLine("Struct {0:X8} {1:X8}", m.Start, m.Size);
        }

        private static unsafe ref MemoryRegion StructRef1_test(MemoryRegion* refPtr)
        {
            return ref Abanu.Kernel.Unsafe.As<MemoryRegion, MemoryRegion>(ref *refPtr);
        }

        private static unsafe void StructRef2()
        {
            var reg = new MemoryRegion(0x55, 0x66);
            ref MemoryRegion m = ref Abanu.Kernel.Unsafe.AsRef<MemoryRegion>(&reg);
            KernelMessage.WriteLine("Struct2 {0:X8} {1:X8}", m.Start, m.Size);
        }

        private static unsafe void StructRef3()
        {
            var reg = new MemoryRegion(0x77, 0x88);
            ref MemoryRegion m = ref Abanu.Kernel.Unsafe.AsRef<MemoryRegion>(&reg);
            var ptr = Abanu.Kernel.Unsafe.AsPointer(ref m);
            var ptr2 = (MemoryRegion*)ptr;
            KernelMessage.WriteLine("Struct2 {0:X8} {1:X8}", ptr2->Start, ptr2->Size);
        }

        private static unsafe void SizeOf()
        {
            //var size3 = Abanu.Kernel.Unsafe.SizeOf<MemoryRegion>();
            //var size1 = Abanu.Kernel.Unsafe.SizeOf<int>();
            //var size2 = Abanu.Kernel.Unsafe.SizeOf<byte>();
            //KernelMessage.WriteLine("Size {0:X8} {1:X8} {2:X8}", size1, size2, size3);
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
