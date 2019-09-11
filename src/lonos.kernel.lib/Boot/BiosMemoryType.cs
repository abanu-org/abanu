// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

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
