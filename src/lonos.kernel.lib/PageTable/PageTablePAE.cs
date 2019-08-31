using System;
using Mosa.Runtime;
using Mosa.Runtime.x86;

using System.Runtime.InteropServices;

namespace lonos.Kernel.Core.PageManagement
{
    /// <summary>
    /// Page Table
    /// </summary>
    public unsafe class PageTablePAE : PageTable
    {

        public uint AddrPageDirectoryPT;
        public uint AddrPageDirectory;
        public uint AddrPageTable;

        public const uint EntriesPerPTTable = 4;
        public const uint PagesPerDictionaryEntry = 512;
        public const uint EntriesPerPageEntryEntry = 512;

        public const ulong InitialAddressableVirtMemory = 0x100000000; // 4GB
        public const uint InitialPageTableEntries = (uint)(InitialAddressableVirtMemory / 4096); // pages for 4GB
        public const uint InitialDirectoryEntries = InitialPageTableEntries / PagesPerDictionaryEntry; // pages for 4GB
        public const uint InitialDirectoryPTEntries = 4;

        private const uint InitalPageDirectorySize = PageDirectoryEntry.EntrySize * InitialDirectoryEntries;
        private const uint InitalPageTableSize = PageTableEntry.EntrySize * InitialPageTableEntries;
        private const uint InitalPageDirectoryPTSize = 4096;

        public override PageTableType Type => PageTableType.PAE;
        public override USize InitalMemoryAllocationSize => InitalPageDirectoryPTSize + InitalPageDirectorySize + InitalPageTableSize;
        public override Addr GdtAddr => AddrPageDirectory;

        private void SetAddress(Addr entriesAddr)
        {
            AddrPageDirectoryPT = entriesAddr;
            AddrPageDirectory = entriesAddr + InitalPageDirectoryPTSize;
            AddrPageTable = entriesAddr + InitalPageDirectoryPTSize + InitalPageDirectorySize;
        }

        /// <summary>
        /// Sets up the PageTable
        /// </summary>
        public override void Setup(Addr entriesAddr)
        {
            SetupBasicStructure(entriesAddr);

            // Set CR3 register on processor - sets page directory
            KernelMessage.WriteLine("Set CR3 to {0:X8}", AddrPageDirectoryPT);
            Flush();

            KernelMessage.Write("Enable Paging... ");

            if (KConfig.UseKernelMemoryProtection)
                EnableKernelWriteProtection();

            // Enable PAE
            Native.SetCR4(Native.GetCR4() | 0x20);

            // Set CR0 register on processor - turns on virtual memory
            Native.SetCR0(Native.GetCR0() | 0x80000000);

            KernelMessage.WriteLine("Done");
        }

        public override void UserProcSetup(Addr entriesAddr)
        {
            SetupBasicStructure(entriesAddr);
        }

        private void SetupBasicStructure(Addr entriesAddr)
        {
            KernelMessage.WriteLine("Setup PageTable");
            SetAddress(entriesAddr);

            MemoryOperation.Clear4(AddrPageDirectoryPT, InitalMemoryAllocationSize);

            PrintAddress();

            // uint mask = 0x00004000;
            // uint v1 = 0x00000007;
            // uint r1 = v1.SetBits(12, 52, mask, 12);

            // ulong v2 = v1;
            // ulong r2 = v2.SetBits(12, 52, mask, 12);
            // uint r2Int = (uint)r2;

            // KernelMessage.WriteLine("r1: {0:X8}", r1);
            // KernelMessage.WriteLine("r2: {0:X8}", r2Int);

            // Setup Page Directory
            PageDirectoryPointerTableEntry* pdpt = (PageDirectoryPointerTableEntry*)AddrPageDirectoryPT;
            PageDirectoryEntry* pde = (PageDirectoryEntry*)AddrPageDirectory;
            PageTableEntry* pte = (PageTableEntry*)AddrPageTable;

            KernelMessage.WriteLine("Total PDPT: {0}", InitialDirectoryPTEntries);
            KernelMessage.WriteLine("Total Page Dictionary Entries: {0}", InitialDirectoryEntries);
            KernelMessage.WriteLine("Total Page Table Entries: {0}", InitialPageTableEntries);

            for (int pidx = 0; pidx < InitialPageTableEntries; pidx++)
            {
                pte[pidx] = new PageTableEntry();
                pte[pidx].Present = true;
                pte[pidx].Writable = true;
                pte[pidx].User = true;
                pte[pidx].PhysicalAddress = (uint)(pidx * 4096);
            }

            for (int didx = 0; didx < InitialDirectoryEntries; didx++)
            {
                pde[didx] = new PageDirectoryEntry();
                pde[didx].Present = true;
                pde[didx].Writable = true;
                pde[didx].User = true;
                pde[didx].PageTableEntry = &pte[didx * PagesPerDictionaryEntry];
            }

            for (int ptidx = 0; ptidx < InitialDirectoryPTEntries; ptidx++)
            {
                pdpt[ptidx] = new PageDirectoryPointerTableEntry();
                pdpt[ptidx].Present = true;
                pdpt[ptidx].PageDirectoryEntry = &pde[ptidx * PagesPerDictionaryEntry];
            }

            // Unmap the first page for null pointer exceptions
            MapVirtualAddressToPhysical(0x0, 0x0, false);
        }

        private void PrintAddress()
        {
            KernelMessage.WriteLine("PageDirectoryPT: {0:X8}", AddrPageDirectoryPT);
            KernelMessage.WriteLine("PageDirectory: {0:X8}", AddrPageDirectory);
            KernelMessage.WriteLine("PageTable: {0:X8}", AddrPageTable);
        }

        private bool WritableAddress(uint physAddr)
        {
            return false;
        }

        public override void KernelSetup(Addr entriesAddr)
        {
            SetAddress(entriesAddr);
            //PrintAddress();
        }

        public PageTableEntry* GetTableEntry(uint forVirtualAddress)
        //public static PageTableEntry* GetTableEntry(uint forVirtualAddress)
        {
            var pageNum = forVirtualAddress >> 12;
            PageTableEntry* table = (PageTableEntry*)AddrPageTable;
            return &table[pageNum];
        }

        /// <summary>
        /// Maps the virtual address to physical.
        /// </summary>
        /// <param name="virtualAddress">The virtual address.</param>
        /// <param name="physicalAddress">The physical address.</param>
        public override void MapVirtualAddressToPhysical(Addr virtualAddress, Addr physicalAddress, bool present = true)
        {
            //FUTURE: traverse page directory from CR3 --- do not assume page table is linearly allocated

            //Intrinsic.Store32(new IntPtr(Address.PageTable), ((virtualAddress & 0xFFFFF000u) >> 10), physicalAddress & 0xFFFFF000u | 0x04u | 0x02u | (present ? 0x1u : 0x0u));

            // Hint: TableEntries are  not initialized. So set every field
            // Hint: All Table Entries have a known location (yet).
            // FUTURE: Change this! Allocate dynamicly

            var entry = GetTableEntry(virtualAddress);
            entry->Present = present;
            entry->Writable = true;
            entry->User = true;
            entry->PhysicalAddress = physicalAddress;

            //Native.Invlpg(virtualAddress); Not implemented in MOSA yet

            // workarround:
            //Flush();
        }

        /// <summary>
        /// Gets the physical memory.
        /// </summary>
        /// <param name="virtualAddress">The virtual address.</param>
        /// <returns></returns>
        public override Addr GetPhysicalAddressFromVirtual(Addr virtualAddress)
        {
            //FUTURE: traverse page directory from CR3 --- do not assume page table is linearly allocated
            return Intrinsic.Load32(new IntPtr(AddrPageTable), ((virtualAddress & 0xFFFFF000u) >> 10)) + (virtualAddress & 0xFFFu);
        }

        public override void SetKernelWriteProtectionForAllInitialPages()
        {
            PageTableEntry* pte = (PageTableEntry*)AddrPageTable;
            for (int index = 0; index < InitialPageTableEntries; index++)
            {
                var e = &pte[index];
                e->Writable = false;
            }
        }

        private void EnableExecutionProtectionInternal()
        {
            // set IA32_EFER.NXE
            const uint EFER = 0xC0000080;
            Native.WrMSR(EFER, Native.RdMSR(EFER) | BitMask.Bit11);
        }
        //[DllImport("x86/lonos.EnableExecutionProtection.o", EntryPoint = "EnableExecutionProtection")]
        //private extern static void EnableExecutionProtectionInternal();

        public override void EnableExecutionProtection()
        {
            EnableExecutionProtectionInternal();
        }

        public override void SetExecutionProtectionForAllInitialPages(LinkedMemoryRegion* currentTextSection)
        {
            // Must be enabled before setting the page bits, otherwise you get a PageFault exception because of using an reserved bit.
            EnableExecutionProtection();

            PageTableEntry* pte = (PageTableEntry*)AddrPageTable;
            for (uint index = 0; index < InitialPageTableEntries; index++)
            {
                var virtAddr = index * 4096;
                var reg = currentTextSection;
                var regionMatches = false;
                while (reg != null)
                {
                    if (reg->Region.Contains(virtAddr))
                    {
                        regionMatches = true;
                        break;
                    }
                    reg = reg->Next;
                }
                if (regionMatches)
                    continue;
                var e = &pte[index];
                e->DisableExecution = true;
            }
        }

        public override void Flush()
        {
            if (PageTable.KernelTable != this)
                return;
            Native.SetCR3(AddrPageDirectoryPT);
        }

        public override void Flush(Addr virtAddr)
        {
            Flush(); //TODO: Use Native.InvPg!
        }

        public unsafe override void SetKernelWriteProtectionForRegion(uint virtAddr, uint size)
        {
            //KernelMessage.WriteLine("Unprotect Memory: Start={0:X}, End={1:X}", virtAddr, virtAddr + size);
            var pages = KMath.DivCeil(size, 4096);
            for (var i = 0; i < pages; i++)
            {
                var entry = GetTableEntry(virtAddr);
                entry->Writable = true;

                virtAddr += 4096;
            }

            Flush();
        }

        public unsafe override void SetExecutableForRegion(uint virtAddr, uint size)
        {
            //KernelMessage.WriteLine("Unprotect Memory: Start={0:X}, End={1:X}", virtAddr, virtAddr + size);
            var pages = KMath.DivCeil(size, 4096);
            for (var i = 0; i < pages; i++)
            {
                var entry = GetTableEntry(virtAddr);
                entry->DisableExecution = false;

                virtAddr += 4096;
            }

            Flush();
        }

        public const byte MAXPHYS = 52;

        [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 8)]
        unsafe public struct PageDirectoryPointerTableEntry
        {
            [FieldOffset(0)]
            private uint data;

            public const byte EntrySize = 8;

            private class Offset
            {
                public const byte Present = 0;
                //public const byte Readonly = 1;
                //public const byte User = 2;
                public const byte WriteThrough = 3;
                public const byte DisableCache = 4;
                //public const byte Accessed = 5;
                //private const byte UNKNOWN6 = 6;
                //public const byte PageSize4Mib = 7;
                //private const byte IGNORED8 = 8;
                //public const byte Custom = 9;
                public const byte Address = 12;
            }

            private const byte AddressBitSize = 52;
            //private const ulong AddressMask = 0xFFFFFFFFFFFFF000u;
            private const uint AddressMask = 0xFFFFF000u;

            private uint PageDirectoryAddress
            {
                get { return data & AddressMask; }
                set
                {
                    Assert.True(value << AddressBitSize == 0, "PageDirectoryEntry.Address needs to be 4k aligned");
                    data = data.SetBits(Offset.Address, AddressBitSize, value, Offset.Address);
                }
            }

            internal PageDirectoryEntry* PageDirectoryEntry
            {
                get { return (PageDirectoryEntry*)PageDirectoryAddress; }
                set { PageDirectoryAddress = (uint)value; }
            }

            public bool Present
            {
                get { return data.IsBitSet(Offset.Present); }
                set { data = data.SetBit(Offset.Present, value); }
            }

            public bool WriteThrough
            {
                get { return data.IsBitSet(Offset.WriteThrough); }
                set { data = data.SetBit(Offset.WriteThrough, value); }
            }

            public bool DisableCache
            {
                get { return data.IsBitSet(Offset.DisableCache); }
                set { data = data.SetBit(Offset.DisableCache, value); }
            }

        }

        [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 8)]
        unsafe public struct PageDirectoryEntry
        {
            [FieldOffset(0)]
            private uint data;

            public const byte EntrySize = 8;

            private class Offset
            {
                public const byte Present = 0;
                public const byte Readonly = 1;
                public const byte User = 2;
                public const byte WriteThrough = 3;
                public const byte DisableCache = 4;
                public const byte Accessed = 5;
                private const byte UNKNOWN6 = 6;
                public const byte PageSize4Mib = 7;
                //private const byte IGNORED8 = 8;
                //public const byte Custom = 9;
                public const byte Address = 12;
                public const byte DisableExecution = 63;
            }

            private const byte AddressBitSize = 52;
            private const uint AddressMask = 0xFFFFF000u;

            private uint PageTableAddress
            {
                get { return data & AddressMask; }
                set
                {
                    Assert.True(value << AddressBitSize == 0, "PageDirectoryEntry.Address needs to be 4k aligned");
                    data = data.SetBits(Offset.Address, AddressBitSize, value, Offset.Address);
                }
            }

            internal PageTableEntry* PageTableEntry
            {
                get { return (PageTableEntry*)PageTableAddress; }
                set { PageTableAddress = (uint)value; }
            }

            public bool Present
            {
                get { return data.IsBitSet(Offset.Present); }
                set { data = data.SetBit(Offset.Present, value); }
            }

            public bool Writable
            {
                get { return data.IsBitSet(Offset.Readonly); }
                set { data = data.SetBit(Offset.Readonly, value); }
            }

            public bool User
            {
                get { return data.IsBitSet(Offset.User); }
                set { data = data.SetBit(Offset.User, value); }
            }

            public bool WriteThrough
            {
                get { return data.IsBitSet(Offset.WriteThrough); }
                set { data = data.SetBit(Offset.WriteThrough, value); }
            }

            public bool DisableCache
            {
                get { return data.IsBitSet(Offset.DisableCache); }
                set { data = data.SetBit(Offset.DisableCache, value); }
            }

            public bool Accessed
            {
                get { return data.IsBitSet(Offset.Accessed); }
                set { data = data.SetBit(Offset.Accessed, value); }
            }

            public bool PageSize4Mib
            {
                get { return data.IsBitSet(Offset.PageSize4Mib); }
                set { data = data.SetBit(Offset.PageSize4Mib, value); }
            }

            // Temp Fix, because of compiler bug!
            public bool DisableExecution
            {
                get { return data.IsBitSet(Offset.DisableExecution); }
                set { data = data.SetBit(Offset.DisableExecution, value); }
            }

        }



        [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 8)]
        public struct PageTableEntry
        {
            //[FieldOffset(0)]
            //private ulong Value;

            [FieldOffset(0)]
            private uint Value;

            [FieldOffset(4)]
            private uint Value2;

            public const byte EntrySize = 8;

            private class Offset
            {
                public const byte Present = 0;
                public const byte Readonly = 1;
                public const byte User = 2;
                public const byte WriteThrough = 3;
                public const byte DisableCache = 4;
                public const byte Accessed = 5;
                public const byte Dirty = 6;
                private const byte SIZE0 = 7;
                public const byte Global = 8;
                //public const byte Custom = 9;
                public const byte Address = 12;
                public const byte DisableExecution = 63;
            }

            //private const byte AddressBitSize = 52;
            //private const ulong AddressMask = 0xFFFFFFFFFFFFF000u;

            private const byte AddressBitSize = 20;
            private const uint AddressMask = 0xFFFFF000u;

            public static bool Debug = false;

            /// <summary>
            /// 4k aligned physical address
            /// </summary>
            public uint PhysicalAddress
            {
                get { return Value & AddressMask; }
                set
                {
                    Assert.True(value << AddressBitSize == 0, "PageTableEntry.PhysicalAddress needs to be 4k aligned");
                    //Debug = true;
                    //if (Debug)
                    //    KernelMessage.WriteLine("Value: {0:X8}, Addr: {1:X8}", (uint)Value, (uint)value);
                    Value = Value.SetBits(Offset.Address, AddressBitSize, value, Offset.Address);
                    //if (Debug)
                    //    KernelMessage.WriteLine("new Value: {0:X8}", (uint)Value);
                }
            }

            public bool Present
            {
                get { return Value.IsBitSet(Offset.Present); }
                set { Value = Value.SetBit(Offset.Present, value); }
            }

            public bool Writable
            {
                get { return Value.IsBitSet(Offset.Readonly); }
                set
                {
                    Value = Value.SetBit(Offset.Readonly, value);
                }
            }

            public bool User
            {
                get { return Value.IsBitSet(Offset.User); }
                set { Value = Value.SetBit(Offset.User, value); }
            }

            public bool WriteThrough
            {
                get { return Value.IsBitSet(Offset.WriteThrough); }
                set { Value = Value.SetBit(Offset.WriteThrough, value); }
            }

            public bool DisableCache
            {
                get { return Value.IsBitSet(Offset.DisableCache); }
                set { Value = Value.SetBit(Offset.DisableCache, value); }
            }

            public bool Accessed
            {
                get { return Value.IsBitSet(Offset.Accessed); }
                set { Value = Value.SetBit(Offset.Accessed, value); }
            }

            public bool Global
            {
                get { return Value.IsBitSet(Offset.Global); }
                set { Value = Value.SetBit(Offset.Global, value); }
            }

            public bool Dirty
            {
                get { return Value.IsBitSet(Offset.Dirty); }
                set { Value = Value.SetBit(Offset.Dirty, value); }
            }

            //public bool DisableExecution
            //{
            //    get { return Value.IsBitSet(Offset.DisableExecution); }
            //    set { Value = Value.SetBit(Offset.DisableExecution, value); }
            //}

            public bool DisableExecution
            {
                get
                {
                    //return dataLong.IsBitSet(Offset.DisableExecution);
                    return Value2 == 0x80000000u;
                }
                set
                {
                    //KernelMessage.WriteLine("sdf");
                    //data = dataLong.SetBit(Offset.DisableExecution, value);
                    if (value)
                    {
                        Value2 = 0x80000000u;
                    }
                    else
                    {
                        Value2 = 0;

                    }
                }
            }

        }
    }


}
