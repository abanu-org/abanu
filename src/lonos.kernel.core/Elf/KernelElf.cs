using System;
using Mosa.Runtime.x86;
using System.Runtime.InteropServices;
using Mosa.Kernel.x86;

namespace lonos.kernel.core
{

    unsafe public static class KernelElf
    {
        public static ElfHelper Main;
        public static ElfHelper Native;

        public static void Setup()
        {
            KernelMessage.WriteLine("Setup ELF Headers");
            KernelMessage.WriteLine("Image Header:");
            KernelMemory.DumpToConsoleLine(0x500000, 124);

            Main = new ElfHelper
            {
                SectionHeaderArray = (ElfSectionHeader*)Multiboot.multiBootInfo->ElfSectionHeader->Addr,
                StringTableSectionHeaderIndex = Multiboot.multiBootInfo->ElfSectionHeader->Shndx,
                SectionHeaderCount = Multiboot.multiBootInfo->ElfSectionHeader->Count
            };
            Main.Init();

            var nativeSec = Main.GetSectionHeader("native");
            var nativeElfAddr = Main.GetSectionPhysAddr(nativeSec);

            var nativeElf = (ElfHeader*)nativeElfAddr;
            var str = (NullTerminatedString*)nativeElfAddr;

            KernelMessage.WriteLine("Found embedded ELF at {0:X8}", nativeElfAddr);

            Native = new ElfHelper
            {
                PhyOffset = nativeElfAddr,
                SectionHeaderArray = (ElfSectionHeader*)(nativeElfAddr + nativeElf->ShOff),
                SectionHeaderCount = nativeElf->ShNum,
                StringTableSectionHeaderIndex = nativeElf->ShStrNdx
            };
            Native.Init();

        }
    }

}
