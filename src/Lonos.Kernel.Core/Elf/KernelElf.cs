// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Runtime.InteropServices;
using Lonos.Kernel.Core.Diagnostics;
using Mosa.Runtime.x86;

namespace Lonos.Kernel.Core.Elf
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

            Main = FromAddress(Address.KernelElfSectionVirt);
            Native = FromSectionName("native");
        }

        public static unsafe ElfHelper FromAddress(Addr elfStart)
        {
            var elfHeader = (ElfHeader*)elfStart;

            if (elfHeader->Ident1 != ElfHeader.Magic1)
            {
                KernelMessage.WriteLine("No valid ELF found at {0:X8}", elfStart);
                // TODO: Throw Exception
            }

            if (KConfig.TraceELF)
                KernelMessage.WriteLine("Found ELF at {0:X8}", elfStart);

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
            return FromAddress(addr);
        }

    }

}
