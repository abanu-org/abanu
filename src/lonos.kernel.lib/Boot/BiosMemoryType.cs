using System;
namespace Lonos.Kernel.Core.Boot
{
    public enum BIOSMemoryMapType : byte
    {
        Usable = 1,
        Reserved = 2,
        ACPI_Relaimable = 3,
        ACPI_NVS_Memory = 4,
        BadMemory = 5,
    }
}
