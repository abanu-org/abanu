// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Runtime.InteropServices;
using lonos.Kernel.Core.Diagnostics;
using Mosa.Kernel.x86;
using Mosa.Runtime.x86;

namespace lonos.Kernel.Core.Elf
{

    public static unsafe class KernelElf
    {
        public static ElfHelper Main;
        public static ElfHelper Native;

        public static void Setup()
        {
            KernelMessage.WriteLine("Setup ELF Headers");
            //KernelMessage.WriteLine("Image Header:");
            //KernelMemory.DumpToConsoleLine(Address.KernelElfSection, 124);

            Main = FromAddress(Address.KernelElfSection);
            Native = FromSectionName("native");
        }

        /// <summary>
        /// Currently unused, because Kernel is loaded via from kernel.loader
        /// </summary>
        /*unsafe static ElfHelper FromMultiBootInfo(MultiBootInfo* multiBootInfo)
        {
            var helper = new ElfHelper
            {
                SectionHeaderArray = (ElfSectionHeader*)multiBootInfo->ElfSectionHeader->Addr,
                StringTableSectionHeaderIndex = multiBootInfo->ElfSectionHeader->Shndx,
                SectionHeaderCount = multiBootInfo->ElfSectionHeader->Count
            };
            helper.Init();
            return helper;
        }*/

        private static unsafe ElfHelper FromAddress(Addr elfStart)
        {
            var elfHeader = (ElfHeader*)elfStart;

            if (elfHeader->Ident1 != ElfHeader.Magic1)
            {
                KernelMessage.WriteLine("No valid ELF found at {0:X8}", elfStart);
                // TODO: Throw Excetion
            }
            var helper = new ElfHelper
            {
                PhyOffset = elfStart,
                SectionHeaderArray = (ElfSectionHeader*)(elfStart + elfHeader->ShOff),
                SectionHeaderCount = elfHeader->ShNum,
                StringTableSectionHeaderIndex = elfHeader->ShStrNdx,
            };
            helper.Init();
            return helper;
        }

        public static unsafe ElfHelper FromSectionName(string name)
        {
            var sec = Main.GetSectionHeader(name);
            if (sec == null)
                Panic.Error("Could not find section " + name);
            var addr = Main.GetSectionPhysAddr(sec);
            KernelMessage.WriteLine("Found embedded ELF at {0:X8}", addr);
            return FromAddress(addr);
        }

    }

}
