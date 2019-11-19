// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System;
using Abanu.Kernel.Core.PageManagement;

namespace Abanu.Kernel.Core.Boot
{
    public static class BootInfo
    {

        /// <summary>
        /// BootInfo passed from the loader
        /// </summary>
        public static unsafe BootInfoHeader* Header;

        public static bool Present;

        public static unsafe void SetupStage1()
        {
            Header = (BootInfoHeader*)Address.KernelBootInfo;
            ApplyAddresses();
        }

        public static unsafe void SetupStage2()
        {
            if (Header->Magic != BootInfoHeader.BootInfoMagic)
            {
                Present = false;
                KernelMessage.WriteLine("bootinfo not present");
                return;
            }

            Present = true;
            KernelMessage.WriteLine("bootinfo present");

            var mapLen = Header->MemoryMapLength;
            KernelMessage.WriteLine("Maps: {0}", mapLen);
            for (uint i = 0; i < mapLen; i++)
            {
                var mm = Header->MemoryMapArray[i];
                KernelMessage.WriteLine("Map Start={0:X8}, Size={1:X8}, Type={2}", mm.Start, mm.Size, (uint)mm.Type);
            }
        }

        private static unsafe void ApplyAddresses()
        {
            GDT.KernelSetup(GetMap(BootInfoMemoryType.GDT)->Start);
            PageTable.ConfigureType(Header->PageTableType);
            PageTable.KernelTable.KernelSetup(GetMap(BootInfoMemoryType.PageTable)->Start);
        }

        /// <summary>
        /// Gets a known memory region by type
        /// </summary>
        public static unsafe BootInfoMemory* GetMap(BootInfoMemoryType type)
        {
            var mapLen = Header->MemoryMapLength;
            for (uint i = 0; i < mapLen; i++)
            {
                if (Header->MemoryMapArray[i].Type == type)
                    return &Header->MemoryMapArray[i];
            }
            return null;
        }

    }
}
