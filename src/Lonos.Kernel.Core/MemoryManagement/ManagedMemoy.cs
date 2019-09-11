// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using Mosa.Kernel.x86;
using Mosa.Runtime;
using Mosa.Runtime.Plug;
using Mosa.Runtime.x86;

namespace Lonos.Kernel.Core.MemoryManagement
{

    internal static class ManagedMemoy
    {

        public static void InitializeGCMemory()
        {
            // Wipe GCMemory from Bootloader
            MemoryOperation.Clear4(Address.GCInitialMemory, Address.GCInitialMemorySize);
        }

        [Plug("Mosa.Runtime.GC::AllocateMemory")]
        private static unsafe IntPtr AllocateMemoryPlug(uint size)
        {
            return AllocateMemory(size);
        }

        internal static bool UseAllocator;

        public static uint EarlyBootBytesUsed => currentSize;

        private static uint currentSize;
        public static uint AllocationCount;

        public static IntPtr AllocateMemory(uint size)
        {
            AllocationCount++;

            //var col = Screen.column;
            //var row = Screen.row;
            //Screen.column = 0;
            //Screen.Goto(0, 35);
            //Screen.Write("AllocCount: ");
            //Screen.Write(AllocationCount);
            //Screen.Goto(1, 35);
            //Screen.row = row;
            //Screen.column = col;

            if (UseAllocator)
                return Memory.Allocate(size, GFP.KERNEL);

            return AllocateMemory_EarlyBoot(size);
        }

        private static IntPtr AllocateMemory_EarlyBoot(uint size)
        {
            var cSize = currentSize;
            currentSize += size;

            return (IntPtr)(((uint)Address.GCInitialMemory) + cSize);
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
