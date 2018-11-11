using System;
using Mosa.Runtime.x86;
using System.Runtime.InteropServices;
using Mosa.Kernel.x86;

namespace lonos.kernel.core
{

    unsafe static class KernelElf {

        public static ElfSectionHeader* SectionHeaderArray{
            get{
                return (ElfSectionHeader*)Multiboot.multiBootInfo->ElfSectionHeader->Addr;
            }
        }

        public static uint SectionHeaderCount {
            get{
                return Multiboot.multiBootInfo->ElfSectionHeader->Count;
            }
        }

        public static ElfSectionHeader* StringTableSectionHeader
        {
            get
            {
                return &(SectionHeaderArray[Multiboot.multiBootInfo->ElfSectionHeader->Shndx]);
            }
        }

        public static NullTerminatedString* GetStringByOffset(uint index){
            return (NullTerminatedString*)(((uint)StringTableSectionHeader->Addr)+index);
        } 

    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ElfSectionHeader
    {
        public uint Name;
        public uint Type;
        public uint Flags;
        public uint Addr;
        public uint Offset;
        public uint Size;
        public uint Link;
        public uint Info;
        public uint AddrAlign;
        public uint EntrySize;
    }

}
