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

    public static class NativeCalls
    {
        private static uint prog1Addr;
        private static uint prog2Addr;

        public static void Setup()
        {
            prog1Addr = KernelElf.Native.GetPhysAddrOfSymbol("proc1");
            prog2Addr = KernelElf.Native.GetPhysAddrOfSymbol("proc2");
        }

        public static void proc1(){
            Native.Jmp(prog1Addr);
        }

        public static void proc2()
        {
            Native.Jmp(prog2Addr);
        }

    }

    public unsafe struct ElfHelper
    {

        public ElfSectionHeader* SectionHeaderArray;
        public uint SectionHeaderCount;
        public uint StringTableSectionHeaderIndex;
        public uint PhyOffset;

        public ElfSectionHeader* SymTab;
        public ElfSectionHeader* StrTab;
        public ElfSectionHeader* Text;

        public void Init()
        {
            SymTab = GetSectionHeader(".symtab");
            StrTab = GetSectionHeader(".strtab");
            Text = GetSectionHeader(".text");
        }

        public uint GetPhysAddrOfSymbol(string name)
        {
            var idx = GetSymbolIndex(name);
            if (idx == -1)
                return 0;
            return GetSectionPhysAddr(Text) + GetSymbol((uint)idx)->Value;
        }

        public ElfSymbol* GetSymbol(uint index)
        {
            var symbols = (ElfSymbol*)GetSectionPhysAddr(SymTab);
            return &symbols[index];
        }

        public ElfSymbol* GetSymbol(string name)
        {
            var idx = GetSymbolIndex(name);
            if (idx == -1)
                return (ElfSymbol*)0;
            return GetSymbol((uint)idx);
        }

        public NullTerminatedString* GetSymbolName(uint symIndex)
        {
            return GetStringByOffset(StrTab, symIndex);
        }

        public int GetSymbolIndex(string name)
        {
            for (uint i = 0; i < SymbolCount; i++)
            {
                var sym = GetSymbol(i);
                var symName = GetSymbolName(sym->Name);
                if (symName->Equals(name))
                    return (int)i;
            }
            return -1;
        }

        public uint SymbolCount
        {
            get
            {
                return SymTab->Size / SymTab->EntrySize;
            }
        }

        public ElfSectionHeader* StringTableSectionHeader
        {
            get
            {
                return GetSectionHeader(StringTableSectionHeaderIndex);
            }
        }

        private NullTerminatedString* GetStringByOffset(ElfSectionHeader* section, uint offset)
        {
            return (NullTerminatedString*)((GetSectionPhysAddr(section)) + offset);
        }

        public NullTerminatedString* GeSectionName(ElfSectionHeader* section)
        {
            return GetStringByOffset(StringTableSectionHeader, section->Name);
        }

        public ElfSectionHeader* GetSectionHeader(uint index)
        {
            return &SectionHeaderArray[index];
        }

        public int GetSectionHeaderIndexByName(string name)
        {
            for (uint i = 0; i < SectionHeaderCount; i++)
            {
                var sec = GetSectionHeader(i);
                var secName = GeSectionName(sec);
                if (secName->Equals(name))
                    return (int)i;
            }
            return -1;
        }

        public ElfSectionHeader* GetSectionHeader(string name)
        {
            var idx = GetSectionHeaderIndexByName(name);
            if (idx == -1)
            {
                Debug.WriteLine(name);
                return (ElfSectionHeader*)0;
            }
            return GetSectionHeader((uint)idx);
        }

        public uint GetSectionPhysAddr(ElfSectionHeader* section)
        {
            if (PhyOffset > 0)
                return section->Offset + PhyOffset;
            else
                return section->Addr + PhyOffset;
        }

    }

    [StructLayout(LayoutKind.Explicit)]
    public struct ElfSymbol
    {
        [FieldOffset(0)] public uint Name;
        [FieldOffset(4)] public uint Value;
        [FieldOffset(8)] public uint Size;
        [FieldOffset(12)] public byte Info;
        [FieldOffset(13)] public byte Other;
        [FieldOffset(14)] public ushort ShNdx;
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

    [StructLayout(LayoutKind.Explicit)]
    public struct ElfHeader
    {
        public static uint Magic1 = 0x464c457f; //0x7f + "ELF"

        [FieldOffset(0)] public uint Ident1;
        [FieldOffset(4)] public uint Ident2;
        [FieldOffset(8)] public uint Ident3;
        [FieldOffset(12)] public uint Ident4;
        [FieldOffset(16)] public ushort Type;
        [FieldOffset(18)] public ushort Machine;
        [FieldOffset(20)] public uint Version;
        [FieldOffset(24)] public uint Entry;
        [FieldOffset(28)] public uint PhOff;
        [FieldOffset(32)] public uint ShOff;
        [FieldOffset(36)] public uint Flags;
        [FieldOffset(40)] public ushort EhSize;
        [FieldOffset(42)] public ushort PhEntSize;
        [FieldOffset(44)] public ushort PhNum;
        [FieldOffset(46)] public ushort ShEntSize;
        [FieldOffset(48)] public ushort ShNum;
        [FieldOffset(50)] public ushort ShStrNdx;
    }

}
