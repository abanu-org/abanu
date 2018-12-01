using System;

namespace lonos.kernel.core
{

    public unsafe struct BootInfoHeader
    {
        // Custom, Random Number. Kernel uses is to detect of BootInfo is available
        public const uint BootInfoMagic = 0x52F8E5E1;

        public uint Magic;

        public BootInfoMemory* MemoryMapArray;
        public uint MemoryMapLength;

        /// <summary>
        /// This is only the Heap for the BootInfo. It's not the Kernel's Heap.
        /// </summary>
        public Addr HeapStart;
        public USize HeapSize;

        public bool VBEPresent;
        public uint VBEMode;

        public BootInfoFramebufferInfo FbInfo;
    }

    public struct BootInfoFramebufferInfo
    {
        public Addr FbAddr;
        public uint FbPitch;
        public uint FbWidth;
        public uint FbHeight;
        public byte FbBpp;
        public byte FbType;
        public uint ColorInfo;
    }
}
