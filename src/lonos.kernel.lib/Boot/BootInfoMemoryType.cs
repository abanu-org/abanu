using System;
namespace lonos.kernel.core
{
    public enum BootInfoMemoryType : byte
    {
        Unknown = 0,
        Usable = 1,
        Reserved = 2,
        ACPI_Relaimable = 3,
        ACPI_NVS_Memory = 4,
        BadMemory = 5,
        OriginalKernelElfImage = 6,
        KernelElf = 7,
        BootInfoHeader = 8,
        BootInfoHeap = 9,
    }
}
