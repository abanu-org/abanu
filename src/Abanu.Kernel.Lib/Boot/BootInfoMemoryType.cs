// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;

namespace Abanu.Kernel.Core.Boot
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
        //PageDirectory = 11,
        PageTable = 12,
        InitialStack = 13,
        InitialGCMemory = 14,

        /// <summary>
        /// like <see cref="Reserved"/>, but added manually.
        /// </summary>
        CustomReserved = 15,

        // Kernel side

        LoaderBinary = 16,
        KernelMemoryMap = 17,
        PageFrameAllocator = 18,

        /// <summary>
        /// Page manager will avoid this region, even if usable. Can overlap with other regions.
        /// </summary>
        KernelReserved = 19,
        IDT = 20,
        TSS = 21,

        KernelElfVirt = 30,
        KernelTextSegment = 30,
        KernelROdataSegment = 31,
        KernelBssSegment = 32,
        KernelDataSegment = 33,

    }
}
