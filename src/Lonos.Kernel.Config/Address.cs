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

        /// <summary>
        /// Initial Stack address for the Loader and Kernel.
        /// TODO: Set dynamically
        /// </summary>
        public const uint InitialStack = 0x00A00000; // 10MB (stack grows down)

        /// <summary>
        /// Defines from where early allocations can begin.
        /// </summary>
        private const uint InitialAllocStart = KernelBasePhys + OriginalKernelElfSize + (128 * 4096); // 64 MB

        /// <summary>
        /// Defines the where early GC memory can taken from.
        /// Currently, Loader and Kernel share the same region (Kernel will overwrite it).
        /// </summary>
        public const uint GCInitialMemory = InitialAllocStart;

        /// <summary>
        /// Defines the size of early GC Memory
        /// </summary>
        public const uint GCInitialMemorySize = 1024 * 1024 * 3; // 3MB

        /// <summary>
        /// Defines the where the loader takes early pages can taken from.
        /// </summary>
        public const uint InitialLoaderDynamicPage = InitialAllocStart + GCInitialMemorySize;

        /// <summary>
        /// Base address where the loader .text section begins (first instruction).
        /// </summary>
        public const uint LoaderBasePhys = 0x00200000;  // 2MB

        /// <summary>
        /// Size of Lonos.OS.Core.x86.bin
        /// </summary>
        public const uint OriginalKernelElfSize = 30 * 1024 * 1024;

        /// <summary>
        /// Base physical address where the kernel .text section begins (first instruction).
        /// It's mapped from <see cref="KernelBaseVirt"/>
        /// </summary>
        public const uint KernelBasePhys = OriginalKernelElfSection + OriginalKernelElfSize + (128 * 4096);  // 20MB

        /// <summary>
        /// Base virtual address where the kernel .text section begins (first instruction).
        /// It's mapped to <see cref="KernelBasePhys"/>
        /// </summary>
        public const uint KernelBaseVirt = 0xC0100000;  // 3GB+1MB

        /// <summary>
        /// Base physical address where the kernel ELF begins (first byte of ELF file).
        /// The Loader will copy it from <see cref="OriginalKernelElfSection"/> and mapped to <see cref="KernelElfSectionVirt"/>
        /// </summary>
        public const uint KernelElfSectionPhys = KernelBasePhys - 0x1000; // 20MB+4KB

        /// <summary>
        /// Base virtual address where the kernel ELF begins (first byte of ELF file).
        /// Its mapped to <see cref="KernelElfSectionPhys"/>
        /// </summary>
        public const uint KernelElfSectionVirt = KernelBaseVirt - 0x1000;

        /// <summary>
        /// Loader will pass boot informations here for the Kernel
        /// </summary>
        public const uint KernelBootInfo = InitialStack + 0x1000; // 10MB+4KB

        /// <summary>
        /// Multiboot will place the ELF here. The Loader will copy the data to <see cref="KernelElfSectionPhys"/>
        /// </summary>
        public const uint OriginalKernelElfSection = KernelBootInfo + 0x1000;  //10MB+8KB

        /// <summary>
        /// Reserved region for identity mapping (phys=virt).
        /// </summary>
        public const uint IdentityMapStart = 0x0C000000; //192mb

        /// <summary>
        /// Start of kernel virtual memory allocation.
        /// </summary>
        public const uint VirtMapStart = 0x40000000; //1gb

        /// <summary>
        /// Address of InterruptControlBlock struct. Holds important informations while Task Switching, like Kernel Page Table address.
        /// </summary>
        public const uint InterruptControlBlock = 0xC0000000;

        /// <summary>
        /// Avoiding the use of the first megabyte of RAM
        /// </summary>
        public const uint ReserveMemory = 0x00100000;  // 1MB

        /// <summary>
        /// Avoiding the last page of Memory, to prevent uint overflow.
        /// </summary>
        public const uint MaximumMemory = 0xFFFFF000;  // 4GB

        public const uint UnitTestStack = 0x00004000;  // 4KB (stack grows down)
        public const uint UnitTestQueue = 0x01E00000;  // 30MB [Size=2MB] - previous: 5KB [Size=1KB] 0x00005000
        public const uint DebuggerBuffer = 0x00010000;  // 16KB [Size=64KB]

        public const uint AppBaseVirt = 0xA0100000;  // 2GB+1MB
        public const uint AppBaseVirt2 = 0xB0100000;  // 2GB+1MB
    }

}
