// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lonos.Kernel.Core
{
    public static class PerformanceCounter
    {
        public static ulong KernelBootStartCycles { get; private set; }

        public static void Setup()
        {
            //KernelBootStartCycles = CpuCyclesSinceSystemBoot();
        }

        public static void Setup(ulong kernelBootStartCycles)
        {
            // It will generate invalid opcode!!
            //_KernelBootStartCycles = kernelBootStartCycles;
        }

        [DllImport("x86/Lonos.CpuCyclesSinceBoot.o", EntryPoint = "CpuCyclesSinceBoot")]
        public static extern ulong CpuCyclesSinceSystemBoot();

        public static ulong CpuCyclesSinceKernelBoot()
        {
            //return CpuCyclesSinceSystemBoot() - KernelBootStartCycles;
            return CpuCyclesSinceSystemBoot() - 12500000000L;
        }

    }
}
