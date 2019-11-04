// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Runtime.InteropServices;
using Abanu.Kernel.Core.Elf;
using Mosa.Runtime.x86;

namespace Abanu.Kernel.Core.External
{

    public static class NativeCalls
    {
        private static uint prog1Addr;
        private static uint prog2Addr;
        private static uint bochsDebugAddr;

        public static void Setup()
        {
            KernelMessage.WriteLine("Setup Native Calls");

            // TODO: VirtAddr!
            // TODO: SetExecutable!
            //PageTable.SetExecutableForRegion(...);

            prog1Addr = KernelElf.Native.GetPhysAddrOfSymbol("test_proc1");
            prog2Addr = KernelElf.Native.GetPhysAddrOfSymbol("test_proc2");
            bochsDebugAddr = KernelElf.Native.GetPhysAddrOfSymbol("bochs_debug");
        }

        public static void Proc1()
        {
            Native.Call(prog1Addr);
        }

        public static void Proc2()
        {
            Native.Call(prog2Addr);
        }

        public static void BochsDebug()
        {
            Native.Call(bochsDebugAddr);
        }

    }

}
