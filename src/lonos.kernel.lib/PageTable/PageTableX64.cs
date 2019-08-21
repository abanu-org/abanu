using System;
using Mosa.Runtime;
using Mosa.Runtime.x86;

using System.Runtime.InteropServices;

namespace lonos.kernel.core
{
    /// <summary>
    /// Page Table
    /// </summary>
    public unsafe static class PageTableX64
    {

        public static uint AddrPageDirectoryPT;
        public static uint AddrPageDirectory;
        public static uint AddrPageTable;

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

        public const uint InitalMemoryAllocationSize = InitalPageDirectoryPTSize + InitalPageDirectorySize + InitalPageTableSize;

        /// <summary>
        /// Sets up the PageTable
        /// </summary>
        public static void Setup(Addr entriesAddr)
        {
            KernelMessage.WriteLine("Setup PageTable");

            AddrPageDirectoryPT = entriesAddr;
            AddrPageDirectory = entriesAddr + InitialDirectoryPTEntries;
            AddrPageTable = entriesAddr + InitialDirectoryPTEntries + InitalPageDirectorySize;

            PrintAddress();

            // Setup Page Directory
            PageDirectoryPointerTableEntry* pdpt = (PageDirectoryPointerTableEntry*)AddrPageDirectory;
            PageDirectoryEntry* pde = (PageDirectoryEntry*)AddrPageDirectory;
            PageTableEntry* pte = (PageTableEntry*)AddrPageTable;

            KernelMessage.WriteLine("Total Pages: {0}", InitialPageTableEntries);
            KernelMessage.WriteLine("Total Page Dictionary Entries: {0}", InitialDirectoryEntries);

            var pidx = 0;
            var didx = 0;
            for (var ptidx = 0; ptidx < 4; ptidx++)
            {
                for (int didxLocal = 0; didxLocal < InitialDirectoryEntries; didxLocal++)
                {
                    for (int index = 0; index < EntriesPerPageEntryEntry; index++)
                    {
                        pte[pidx] = new PageTableEntry();
                        pte[pidx].Present = true;
                        pte[pidx].Writable = true;
                        pte[pidx].User = true;
                        pte[pidx].PhysicalAddress = (uint)(pidx * 4096);

                        pidx++;
                    }

                    pde[didx] = new PageDirectoryEntry();
                    pde[didx].Present = true;
                    pde[didx].Writable = true;
                    pde[didx].User = true;
                    pde[didx].PageTableEntry = &pte[didx * EntriesPerPageEntryEntry];

                    didx++;
                }

                pdpt[didx] = new PageDirectoryPointerTableEntry();
                pdpt[didx].Present = true;
                pdpt[didx].PageDirectoryEntry = &pde[ptidx * PagesPerDictionaryEntry];
            }

            // TODO/BUG: Why this line will not be printed??
            KernelMessage.WriteLine("Total Page Table Entries: {0}", pidx);

            // Unmap the first page for null pointer exceptions
            MapVirtualAddressToPhysical(0x0, 0x0, false);

            // Set CR3 register on processor - sets page directory
            KernelMessage.WriteLine("Set CR3 to {0:X8}", AddrPageDirectory);
            Flush();

            KernelMessage.Write("Enable Paging... ");

            PageTable.EnableKernelWriteProtection();

            // Enable PAE
            Native.SetCR4(Native.GetCR4() | 0x20);

            // Set CR0 register on processor - turns on virtual memory
            Native.SetCR0(Native.GetCR0() | 0x80000000);

            KernelMessage.WriteLine("Done");
        }

        private static void PrintAddress()
        {
            KernelMessage.WriteLine("PageDirectory: {0:X8}", AddrPageDirectory);
            KernelMessage.WriteLine("PageTable: {0:X8}", AddrPageTable);
        }

        private static bool WritableAddress(uint physAddr)
        {
            return false;
        }

        public static void KernelSetup(Addr entriesAddr)
        {
            AddrPageDirectory = entriesAddr;
            AddrPageTable = entriesAddr + InitalPageDirectorySize;
            //PrintAddress();
        }

        public static PageTableEntry* GetTableEntry(ulong forVirtualAddress)
        {
            var pageNum = forVirtualAddress >> 12;
            PageTableEntry* table = (PageTableEntry*)AddrPageDirectory;
            return &table[pageNum];
        }

        /// <summary>
        /// Maps the virtual address to physical.
        /// </summary>
        /// <param name="virtualAddress">The virtual address.</param>
        /// <param name="physicalAddress">The physical address.</param>
        public static void MapVirtualAddressToPhysical(Addr virtualAddress, Addr physicalAddress, bool present = true)
        {
            //FUTURE: traverse page directory from CR3 --- do not assume page table is linearly allocated

            //Intrinsic.Store32(new IntPtr(Address.PageTable), ((virtualAddress & 0xFFFFF000u) >> 10), physicalAddress & 0xFFFFF000u | 0x04u | 0x02u | (present ? 0x1u : 0x0u));

            // Hint: TableEntries are  not initialized. So set every field
            // Hint: All Table Entries have a known location (yet).
            // FUTURE: Change this! Allocate dynamicly

            var entry = GetTableEntry(virtualAddress);
            entry->PhysicalAddress = physicalAddress;
            entry->Present = present;
            entry->Writable = true;
            entry->User = true;

            //Native.Invlpg(virtualAddress); Not implemented in MOSA yet

            // workarround:
            Flush();
        }

        /// <summary>
        /// Gets the physical memory.
        /// </summary>
        /// <param name="virtualAddress">The virtual address.</param>
        /// <returns></returns>
        public static uint GetPhysicalAddressFromVirtual(IntPtr virtualAddress)
        {
            //FUTURE: traverse page directory from CR3 --- do not assume page table is linearly allocated
            return Intrinsic.Load32(new IntPtr(AddrPageTable), (((uint)virtualAddress.ToInt32() & 0xFFFFF000u) >> 10)) + ((uint)virtualAddress.ToInt32() & 0xFFFu);
        }

        public static void SetKernelWriteProtectionForAllInitialPages()
        {
            PageTableEntry* pte = (PageTableEntry*)AddrPageTable;
            for (int index = 0; index < InitialPageTableEntries; index++)
            {
                var e = &pte[index];
                e->Writable = false;
            }
        }

        public static void Flush()
        {
            Native.SetCR3(AddrPageDirectoryPT);
        }

        public static void Flush(Addr virtAddr)
        {
            Flush(); //TODO: Use Native.InvPg!
        }

        public unsafe static void SetKernelWriteProtectionForRegion(uint virtAddr, uint size)
        {
            //KernelMessage.WriteLine("Unprotect Memory: Start={0:X}, End={1:X}", virtAddr, virtAddr + size);
            var pages = KMath.DivCeil(size, 4096);
            for (var i = 0; i < pages; i++)
            {
                var entry = GetTableEntry(virtAddr);
                entry->Writable = true;

                virtAddr += 4096;
            }

            PageTable.Flush();
        }

        public const byte MAXPHYS = 52;

        [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 8)]
        unsafe public struct PageDirectoryPointerTableEntry
        {
            [FieldOffset(0)]
            private ulong data;

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

            private const byte AddressBitSize = 40;
            private const ulong AddressMask = 0xFFFFFFFFFFFFF000u;

            private ulong PageDirectoryAddress
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
                set { PageDirectoryAddress = (ulong)value; }
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
            private ulong data;

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

            private const byte AddressBitSize = 40;
            private const ulong AddressMask = 0xFFFFFFFFFFFFF000u;

            private ulong PageTableAddress
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
                set { PageTableAddress = (ulong)value; }
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

            public bool DisableExecution
            {
                get { return data.IsBitSet(Offset.DisableExecution); }
                set { data = data.SetBit(Offset.DisableExecution, value); }
            }

        }



        [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 8)]
        public struct PageTableEntry
        {
            [FieldOffset(0)]
            private ulong Value;

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

            private const byte AddressBitSize = 20;
            private const ulong AddressMask = 0xFFFFFFFFFFFFF000u;

            /// <summary>
            /// 4k aligned physical address
            /// </summary>
            public ulong PhysicalAddress
            {
                get { return Value & AddressMask; }
                set
                {
                    Assert.True(value << AddressBitSize == 0, "PageTableEntry.PhysicalAddress needs to be 4k aligned");
                    Value = Value.SetBits(Offset.Address, AddressBitSize, value, Offset.Address);
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
                set { Value = Value.SetBit(Offset.Readonly, value); }
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

            public bool DisableExecution
            {
                get { return Value.IsBitSet(Offset.DisableExecution); }
                set { Value = Value.SetBit(Offset.DisableExecution, value); }
            }

        }
    }


}
