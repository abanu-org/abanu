using System;
namespace lonos.kernel.core
{
    public enum BootInfoMemoryType : byte
    {
        Unknown = 0,
        SystemUsable = 1,
        Reserved = 2,
        ACPI_Relaimable = 3,
        ACPI_NVS_Memory = 4,
        BadMemory = 5,
        OriginalKernelElfImage = 6,
        KernelElf = 7,
        BootInfoHeader = 8,
        BootInfoHeap = 9,
        GDT = 10,
        PageDirectory = 11,
        PageTable = 12,
        InitialStack = 13,
        InitialGCMemory = 14,
        KernelReserved = 16,

        // Kernel side

        KernelMemoryMap = 17,
        PageFrameAllocator = 18,

        KernelElfVirt = 30,
        KernelTextSegment = 30,
        KernelROdataSegment = 31,
        KernelBssSegment = 32,
        KernelDataSegment = 33,

    }
}
