// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Abanu.Kernel.Core
{
    public static class PerformanceCounter
    {
        public static ulong KernelBootStartCycles { get; private set; }

        public static void Setup()
        {
            KernelBootStartCycles = CpuCyclesSinceSystemBoot();
        }

        public static void Setup(ulong kernelBootStartCycles)
        {
            // It will generate invalid opcode!!
            KernelBootStartCycles = kernelBootStartCycles;
        }

        public static bool Initialized => KernelBootStartCycles > 0;

        [DllImport("x86/Abanu.CpuCyclesSinceBoot.o", EntryPoint = "CpuCyclesSinceBoot")]
        public static extern ulong CpuCyclesSinceSystemBoot();

        public static ulong CpuCyclesSinceKernelBoot()
        {
            return CpuCyclesSinceSystemBoot() - KernelBootStartCycles;
            //return CpuCyclesSinceSystemBoot() - 12500000000L;
        }

        public static uint GetReadableCounter()
        {
            return (uint)(CpuCyclesSinceKernelBoot() / 1000000UL);
        }

    }
}
