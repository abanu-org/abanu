// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

namespace Lonos.Kernel.Core
{
    /// <summary>
    /// Static known addresses.
    /// Because Members are accessed before Runtime-Initialization, only constants and Readonly-Properties are allowed
    /// </summary>
    public static class Address
    {

        public const uint InitialStack = 0x00A00000; // 10MB (stack grows down)

        private const uint InitialAllocStart = KernelBasePhys + OriginalKernelElfSize + (128 * 4096); // 64 MB

        public const uint GCInitialMemory = InitialAllocStart;
        public const uint GCInitialMemorySize = 1024 * 1024 * 3; // 3MB

        public const uint InitialDynamicPage = InitialAllocStart + GCInitialMemorySize;
        public const uint LoaderBasePhys = 0x00200000;  // 2MB

        /// <summary>
        /// Lonos.OS.Core.x86.bin
        /// </summary>
        public const uint OriginalKernelElfSize = 30 * 1024 * 1024;

        public const uint KernelBasePhys = OriginalKernelElfSection + OriginalKernelElfSize + (128 * 4096);  // 20MB
        public const uint KernelBaseVirt = 0xC0100000;  // 3GB+1MB
        public const uint KernelElfSection = KernelBasePhys - 0x1000; // 20MB+4KB
        public const uint KernelBootInfo = InitialStack + 0x1000; // 10MB+4KB
        public const uint OriginalKernelElfSection = KernelBootInfo + 0x1000;  //10MB+8KB

        public const uint IdentityMapStart = 0x0C000000; //192mb
        public const uint VirtMapStart = 0x40000000; //1gb

        public const uint InterruptControlBlock = 0xC0000000;

        public const uint ReserveMemory = 0x00100000;  // 1MB
        public const uint MaximumMemory = 0xFFFFFFFF;  // 4GB

        public const uint UnitTestStack = 0x00004000;  // 4KB (stack grows down)
        public const uint UnitTestQueue = 0x01E00000;  // 30MB [Size=2MB] - previous: 5KB [Size=1KB] 0x00005000
        public const uint DebuggerBuffer = 0x00010000;  // 16KB [Size=64KB]

        public const uint AppBaseVirt = 0xA0100000;  // 2GB+1MB
        public const uint AppBaseVirt2 = 0xB0100000;  // 2GB+1MB
    }

}
