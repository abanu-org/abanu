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
