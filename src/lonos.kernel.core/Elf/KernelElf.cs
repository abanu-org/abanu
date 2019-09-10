using System;
using Mosa.Runtime.x86;
using System.Runtime.InteropServices;
using Mosa.Kernel.x86;
using lonos.Kernel.Core.Diagnostics;

namespace lonos.Kernel.Core.Elf
{

    unsafe public static class KernelElf
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

        unsafe static ElfHelper FromAddress(Addr elfStart)
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
                StringTableSectionHeaderIndex = elfHeader->ShStrNdx
            };
            helper.Init();
            return helper;
        }

        public unsafe static ElfHelper FromSectionName(string name)
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
