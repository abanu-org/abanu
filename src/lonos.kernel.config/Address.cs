// Copyright (c) MOSA Project. Licensed under the New BSD License.

namespace lonos.kernel.core
{
    public static class Address
    {
        /// <summary>
        /// The Name of Kernel Entry Method. It's used by the Loader to determine it's address.
        /// </summary>
        public static readonly string KernelEntryName = "System.Void lonos.kernel.core.Start::Main()";

        public const uint InitialStack = 0x000F0000; // ???KB (stack grows down)

        private const uint TempShift = 0x1000000;

        public const uint PageDirectory = TempShift + 0x00B00000;  // 12MB [Size=4KB]
        public const uint GDTTable = TempShift + 0x00B10000;  // 12MB+ [Size=1KB]
        public const uint IDTTable = TempShift + 0x00B11000;  // 12MB+ [Size=1KB]

        public const uint PageFrameAllocator = TempShift + 0x00C00000;  // 13MB [Size=4MB]
        public const uint PageTable = TempShift + 0x01000000;  // 16MB [Size=4MB]
        public const uint VirtualPageAllocator = TempShift + 0x01400000;  // 20MB [Size=32KB]

        public const uint GCInitialMemory = TempShift + 0x03000000;  // 48MB [Size=16MB]
        public const uint GCInitialMemory_BootLoader = TempShift + 0x02000000;  // 32MB [Size=16MB]

        public const uint LoaderBasePhys = 0x00200000;  // 3MB

        public const uint KernelBasePhys = 0x01400000;  // 27MB
        public const uint KernelBaseVirt = 0xC0100000;  // 3GB+1MB
        //public const uint KernelBaseVirt = 0x7000000;  // 3GB+1MB
        public const uint KernelElfSection = KernelBasePhys - 0x1000;
        public const uint OriginalKernelElfSection = 0x00C00000;  //17MB
        public const uint KernelBootInfo = OriginalKernelElfSection - 0x1000;
        public const uint VirtuaMemory = 0x04000000;

        public const uint ReserveMemory = TempShift + 0x04000000;  // 
        //public const uint ReserveMemory = 0x05000000;  // 80MB
        public const uint MaximumMemory = 0xFFFFFFFF;  // 4GB

        public const uint UnitTestStack = 0x00004000;  // 4KB (stack grows down)
        public const uint UnitTestQueue = 0x01E00000;  // 30MB [Size=2MB] - previous: 5KB [Size=1KB] 0x00005000
        public const uint DebuggerBuffer = 0x00010000;  // 16KB [Size=64KB]
    }
}
