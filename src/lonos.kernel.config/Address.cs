
namespace lonos.Kernel.Core
{
    /// <summary>
    /// Static known adresses.
    /// Because Members are accessed before Runtime-Initialization, only constants and Readonly-Properties are allowed
    /// </summary>
    public static class Address
    {

        public const uint InitialStack = 0x00A00000; // 10MB (stack grows down)

        private const uint InitialAllocStart = 0x04000000; // 64 MB

        public const uint GCInitialMemory = InitialAllocStart;
        public const uint GCInitialMemorySize = 1024 * 1024 * 3; // 3MB


        public const uint InitialDynamicPage = InitialAllocStart + GCInitialMemorySize;
        public const uint LoaderBasePhys = 0x00200000;  // 2MB

        public const uint KernelBasePhys = 0x01400000;  // 20MB
        public const uint KernelBaseVirt = 0xC0100000;  // 3GB+1MB
        public const uint KernelElfSection = KernelBasePhys - 0x1000; // 20MB+4KB
        public const uint KernelBootInfo = InitialStack + 0x1000; // 10MB+4KB
        public const uint OriginalKernelElfSection = KernelBootInfo + 0x1000;  //10MB+8KB

        public const uint InterruptControlBlock = 0xC0000000;

        public const uint ReserveMemory = 0x00100000;  // 1MB
        public const uint MaximumMemory = 0xFFFFFFFF;  // 4GB

        public const uint UnitTestStack = 0x00004000;  // 4KB (stack grows down)
        public const uint UnitTestQueue = 0x01E00000;  // 30MB [Size=2MB] - previous: 5KB [Size=1KB] 0x00005000
        public const uint DebuggerBuffer = 0x00010000;  // 16KB [Size=64KB]

        public const uint AppBaseVirt = 0xA0100000;  // 2GB+1MB
    }

}
