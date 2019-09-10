using System;
using Mosa.Runtime.x86;
using System.Runtime.InteropServices;

namespace lonos.Kernel.Core.Elf
{

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
                //KernelMessage.WriteLine("Section not found: {0}", name);
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

        public uint TotalFileSize
        {
            get
            {
                var lastSection = SectionHeaderCount - 1;
                return SectionHeaderArray[lastSection].Offset + SectionHeaderArray[lastSection].Size;
            }
        }

    }

}
