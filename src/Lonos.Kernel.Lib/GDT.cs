// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

/*
 * (c) 2015 MOSA - The Managed Operating System Alliance
 *
 * Licensed under the terms of the New BSD License.
 *
 * Authors:
 *  Phil Garcia (tgiphil) <phil@thinkedge.com>
 *  Stefan Andres Charsley (charsleysa) <charsleysa@gmail.com>
 *  Sebastian Loncar (Arakis) <sebastian.loncar@gmail.com>
 */

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mosa.Runtime;
using Mosa.Runtime.x86;

namespace Lonos.Kernel.Core
{

    public static unsafe class GDT
    {
        private static uint GdtTableAddress;
        private static DescriptorTable* GdtTable;

        private static uint TssAddr;
        private static TaskStateSegmentTable* TssTable;

        /// <summary>
        /// Sets up the GDT table and entries
        /// </summary>
        public static void Setup(Addr addr)
        {
            KernelMessage.Write("Setup GDT...");

            GdtTableAddress = addr;

            GdtTable = (DescriptorTable*)GdtTableAddress;
            GdtTable->Clear();
            GdtTable->AdressOfEntries = GdtTableAddress + DescriptorTable.StructSize;

            //Null segment
            var nullEntry = DescriptorTableEntry.CreateNullDescriptor();
            GdtTable->AddEntry(nullEntry);

            //code segment
            var codeEntry = DescriptorTableEntry.CreateCode(0, 0xFFFFFFFF);
            codeEntry.CodeSegment_Readable = true;
            codeEntry.PriviligeRing = 0;
            codeEntry.Present = true;
            codeEntry.AddressMode = DescriptorTableEntry.EAddressMode.Bits32;
            //codeEntry.CodeSegment_Confirming = true;
            GdtTable->AddEntry(codeEntry);

            //data segment
            var dataEntry = DescriptorTableEntry.CreateData(0, 0xFFFFFFFF);
            dataEntry.DataSegment_Writable = true;
            dataEntry.PriviligeRing = 0;
            dataEntry.Present = true;
            dataEntry.AddressMode = DescriptorTableEntry.EAddressMode.Bits32;
            GdtTable->AddEntry(dataEntry);

            Flush();

            KernelMessage.WriteLine("Done");
        }

        public static TaskStateSegment* Tss;

        public static void SetupUserMode(Addr tssAddr)
        {
            KernelMessage.WriteLine("Setup GDT UserMode");

            if (KConfig.UseTaskStateSegment)
            {
                TssAddr = tssAddr;
                TssTable = (TaskStateSegmentTable*)tssAddr;
                TssTable->Clear();
                TssTable->AdressOfEntries = TssAddr + TaskStateSegmentTable.StructSize;
            }

            //code segment
            var codeEntry = DescriptorTableEntry.CreateCode(0, 0xFFFFFFFF);
            codeEntry.CodeSegment_Readable = true;
            codeEntry.PriviligeRing = 3;
            codeEntry.Present = true;
            codeEntry.AddressMode = DescriptorTableEntry.EAddressMode.Bits32;
            codeEntry.CodeSegment_Confirming = false;
            GdtTable->AddEntry(codeEntry);

            //data segment
            var dataEntry = DescriptorTableEntry.CreateData(0, 0xFFFFFFFF);
            dataEntry.DataSegment_Writable = true;
            dataEntry.PriviligeRing = 3;
            dataEntry.Present = true;
            dataEntry.AddressMode = DescriptorTableEntry.EAddressMode.Bits32;
            GdtTable->AddEntry(dataEntry);

            if (KConfig.UseTaskStateSegment)
            {
                //TSS
                Tss = AddTSS();
                //tss->esp0 = kernelStackPointer;
                Tss->SS0 = 0x10;
                Tss->Trace_bitmap = 0xdfff;

                KernelMessage.WriteLine("Addr of tss: {0:X8}", (uint)Tss);

                var tssEntry = DescriptorTableEntry.CreateTSS(Tss);
                tssEntry.PriviligeRing = 0;
                tssEntry.TSS_AVL = true;
                tssEntry.Present = true;
                GdtTable->AddEntry(tssEntry);
            }

            Flush();

            if (KConfig.UseTaskStateSegment)
            {
                KernelMessage.WriteLine("LoadTaskRegister...");
                //LoadTaskRegister();

                //Debug, for breakpoint
                //clockTicks++;

                //DebugFunction1();

            }

            KernelMessage.WriteLine("Done");
        }

        //[DllImport("x86/lonos.DebugFunction1.o", EntryPoint = "DebugFunction1")]
        //public extern static void DebugFunction1();

        [DllImport("x86/lonos.LoadTaskRegister.o", EntryPoint = "LoadTaskRegister")]
        private static extern void LoadTaskRegister(ushort taskSegmentSelector);

        public static void LoadTaskRegister()
        {
            ushort ts = 0x28;
            if (KConfig.UseUserMode)
                ts |= 0x3;
            LoadTaskRegister(ts);
        }

        private static TaskStateSegment* AddTSS()
        {
            var tss = new TaskStateSegment();
            return TssTable->AddEntry(tss);
        }

        public static void KernelSetup(Addr gdtAddr)
        {
            GdtTableAddress = gdtAddr;
            GdtTable = (DescriptorTable*)GdtTableAddress;
        }

        /// <summary>
        /// Flushes the GDT table
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Flush()
        {
            Native.Lgdt(GdtTableAddress);
            Native.SetSegments(0x10, 0x10, 0x10, 0x10, 0x10);
            Native.FarJump();
        }
    }

    /// <summary>
    /// Global Descriptor Table and Local Descriptor Table
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct DescriptorTable
    {
        [FieldOffset(0)]
        private ushort size;

        [FieldOffset(2)]
        public uint AdressOfEntries;

        public const byte StructSize = 0x06;

        private DescriptorTableEntry* Entries
        {
            get { return (DescriptorTableEntry*)AdressOfEntries; }
            set { AdressOfEntries = (uint)value; }
        }

        internal DescriptorTableEntry* GetEntryRef(ushort index)
        {
            Assert.InRange(index, Length);
            return Entries + index;
        }

        public ushort Length
        {
            get
            {
                if (size == 0)
                    return 0;
                else
                    return (ushort)((size + 1) / DescriptorTableEntry.EntrySize);
            }

            private set
            {
                if (value == 0)
                    size = 0;
                else
                    size = (ushort)((value * DescriptorTableEntry.EntrySize) - 1);
            }
        }

        public void Clear()
        {
            Length = 0;
        }

        public void AddEntry(DescriptorTableEntry source)
        {
            Length++;
            SetEntry((ushort)(Length - 1), source);
        }

        public void SetEntry(ushort index, DescriptorTableEntry source)
        {
            Assert.InRange(index, Length);
            //DescriptorTableEntry.CopyTo(&source, entries + index);
            Entries[index] = source;
        }

        public DescriptorTableEntry GetEntry(ushort index)
        {
            Assert.InRange(index, Length);
            return *(Entries + index);
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct TaskStateSegmentTable
    {
        [FieldOffset(0)]
        private ushort size;

        [FieldOffset(2)]
        public uint AdressOfEntries;

        public const byte StructSize = 0x06;

        private TaskStateSegment* Entries
        {
            get { return (TaskStateSegment*)AdressOfEntries; }
            set { AdressOfEntries = (uint)value; }
        }

        internal TaskStateSegment* GetEntryRef(ushort index)
        {
            Assert.InRange(index, Length);
            return &Entries[index];
        }

        public ushort Length
        {
            get
            {
                if (size == 0)
                    return 0;
                else
                    return (ushort)((size + 1) / TaskStateSegment.EntrySize);
            }

            private set
            {
                if (value == 0)
                    size = 0;
                else
                    size = (ushort)((value * TaskStateSegment.EntrySize) - 1);
            }
        }

        public void Clear()
        {
            Length = 0;
        }

        public TaskStateSegment* AddEntry(TaskStateSegment source)
        {
            Length++;
            return SetEntry((ushort)(Length - 1), source);
        }

        public TaskStateSegment* SetEntry(ushort index, TaskStateSegment source)
        {
            Assert.InRange(index, Length);
            Entries[index] = source;
            return &Entries[index];
        }

        public TaskStateSegment GetEntry(ushort index)
        {
            Assert.InRange(index, Length);
            return Entries[index];
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct DescriptorTableEntry
    {
        [FieldOffset(0)]
        private ushort limitLow;

        [FieldOffset(2)]
        private ushort baseLow;

        [FieldOffset(4)]
        private byte baseMiddle;

        [FieldOffset(5)]
        private byte access;

        [FieldOffset(6)]
        private byte flags;

        [FieldOffset(7)]
        private byte baseHigh;

        private class AccessByteOffset
        {
            public const byte SegmentType = 0; //bits 0-4
            public const byte Ac = 0;
            public const byte RW = 1;
            public const byte DC = 2;
            public const byte Ex = 3;
            public const byte UserDescriptor = 4;
            public const byte Privl = 5;
            public const byte Pr = 7;
        }

        private class FlagsByteOffset
        {
            public const byte Limit = 0;
            public const byte TSS_AVL = 4;
            public const byte LongMode = 5;
            public const byte Sz = 6; // 1 = 32 bit code, 0 = 16 bit
            public const byte Gr = 7;
        }

        public const byte EntrySize = 0x08;

        public static DescriptorTableEntry CreateNullDescriptor()
        {
            return new DescriptorTableEntry();
        }

        private static DescriptorTableEntry Create(bool granularity, uint baseAddress, uint limit)
        {
            var entry = new DescriptorTableEntry() { Granularity = granularity };
            entry.BaseAddress = baseAddress;
            entry.Limit = limit;
            return entry;
        }

        public static DescriptorTableEntry CreateCode(uint baseAddress, uint limit)
        {
            var seg = Create(true, baseAddress, limit);
            seg.IsUserType = true;
            seg.UserDescriptor_Executable = true;
            return seg;
        }

        public static DescriptorTableEntry CreateData(uint baseAddress, uint limit)
        {
            var seg = Create(true, baseAddress, limit);
            seg.IsUserType = true;
            return seg;
        }

        public static DescriptorTableEntry CreateTSS(TaskStateSegment* tss)
        {
            var seg = Create(false, (uint)tss, Core.TaskStateSegment.EntrySize);
            seg.SystemDescriptor_Type = InactiveTask;
            return seg;
        }

        public TaskStateSegment* TaskStateSegment
        {
            get
            {
                return (TaskStateSegment*)BaseAddress;
            }

            set
            {
                BaseAddress = (uint)value;
                Limit = Core.TaskStateSegment.EntrySize;
            }
        }

        public bool IsTaskDescriptor => SystemDescriptor_Type == InactiveTask || SystemDescriptor_Type == BusyTask;

        public bool IsNullDescriptor
        {
            get { return limitLow == 0 && baseLow == 0 && baseMiddle == 0 && access == 0 && flags == 0 && baseHigh == 0; }
        }

        public uint BaseAddress
        {
            get
            {
                return (uint)(baseLow | (baseMiddle << 16) | (baseHigh << 24));
            }

            set
            {
                baseLow = (ushort)(value & 0xFFFF);
                baseMiddle = (byte)((value >> 16) & 0xFF);
                baseHigh = (byte)((value >> 24) & 0xFF);
            }
        }

        public uint Limit
        {
            get
            {
                return (uint)(limitLow | flags.GetBits(FlagsByteOffset.Limit, 4));
            }

            set
            {
                if (Granularity)
                {
                    limitLow = (ushort)(value & 0xFFFF);
                    flags = flags.SetBits(FlagsByteOffset.Limit, 4, (byte)((value >> 16) & 0x0F));
                }
                else
                {
                    limitLow = (ushort)(value & 0xFFFF);
                    //flags = flags.SetBits(FlagsByteOffset.Limit, 4, (byte)((value >> 16) & 0x0F));
                    //flag (limit & 0xF0000) >> 16
                    var highBits = (byte)((value & 0xF0000) >> 16);
                    flags = flags.SetBits(FlagsByteOffset.Limit, 4, highBits);
                }
            }
        }

        //public static void CopyTo(DescriptorTableEntry* source, DescriptorTableEntry* destination)
        //{
        //    Memory.Copy((uint)source, (uint)destination, EntrySize);
        //}

        //public static void Clear(DescriptorTableEntry* entry)
        //{
        //    Memory.Clear((uint)entry, EntrySize);
        //}

        #region AccessByte

        public bool Present
        {
            get { return access.IsBitSet(AccessByteOffset.Pr); }
            set { access = access.SetBit(AccessByteOffset.Pr, value); }
        }

        public bool UserDescriptor_Executable
        {
            get
            {
                CheckSegment();
                return access.IsBitSet(AccessByteOffset.Ex);
            }

            set
            {
                CheckSegment();
                access = access.SetBit(AccessByteOffset.Ex, value);
            }
        }

        public bool IsUserType
        {
            get { return access.IsBitSet(AccessByteOffset.UserDescriptor); }
            set { access = access.SetBit(AccessByteOffset.UserDescriptor, value); }
        }

        private bool DirectionConfirming
        {
            get { return access.IsBitSet(AccessByteOffset.DC); }
            set { access = access.SetBit(AccessByteOffset.DC, value); }
        }

        public bool ReadWrite
        {
            get
            {
                CheckSegment();
                return access.IsBitSet(AccessByteOffset.RW);
            }

            set
            {
                CheckSegment();
                access = access.SetBit(AccessByteOffset.RW, value);
            }
        }

        public bool UserDescriptor_Accessed
        {
            get
            {
                CheckSegment();
                return access.IsBitSet(AccessByteOffset.Ac);
            }

            set
            {
                CheckSegment();

                access = access.SetBit(AccessByteOffset.Ac, value);
            }
        }

        #region UserSegment

        #region CodeSegment

        public bool CodeSegment_Readable
        {
            get
            {
                CheckSegment();
                return UserDescriptor_Executable ? ReadWrite : true;
            }

            set
            {
                CheckSegment();
                Assert.False(!UserDescriptor_Executable && !value, "Read access is always allowed for data segments");

                ReadWrite = value;
            }
        }

        public bool CodeSegment_Confirming
        {
            get { return access.IsBitSet(AccessByteOffset.DC); }
            set { access = access.SetBit(AccessByteOffset.DC, value); }
        }

        #endregion CodeSegment

        #region DataSegment

        public bool DataSegment_Writable
        {
            get
            {
                return UserDescriptor_Executable ? ReadWrite : false;
            }

            set
            {
                Assert.False(UserDescriptor_Executable && value, "Write access is never allowed for code segments");
                ReadWrite = value;
            }
        }

        public bool DataSegment_Direction_ExpandDown
        {
            get { return access.IsBitSet(AccessByteOffset.DC); }
            set { access = access.SetBit(AccessByteOffset.DC, value); }
        }

        #endregion DataSegment

        #endregion UserSegment

        #region SystemDescriptor

        public byte SystemDescriptor_Type
        {
            get { return access.GetBits(AccessByteOffset.SegmentType, 4); }
            set { access = access.SetBits(AccessByteOffset.SegmentType, 4, value); }
        }

        public const byte InactiveTask = 0b_1001;
        public const byte BusyTask = 0b_1011;

        #endregion SystemDescriptor

        public byte PriviligeRing
        {
            get
            {
                return access.GetBits(AccessByteOffset.Privl, 2);
            }
            set
            {
                Assert.False(value > 3, "Privilege ring can't be larger than 3");
                access = access.SetBits(AccessByteOffset.Privl, 2, value);
            }
        }

        private void CheckSegment()
        {
            Assert.True(IsUserType, "This attribute can't accessed with this segment type");
        }

        #endregion AccessByte

        #region Flags

        public bool TSS_AVL
        {
            get
            {
                return flags.IsBitSet(FlagsByteOffset.TSS_AVL);
            }

            set
            {
                flags = flags.SetBit(FlagsByteOffset.TSS_AVL, value);
            }
        }

        private bool LongMode
        {
            get
            {
                return flags.IsBitSet(FlagsByteOffset.LongMode);
            }

            set
            {
                flags = flags.SetBit(FlagsByteOffset.LongMode, value);
                if (value)
                    SizeBit = false;
            }
        }

        private bool SizeBit
        {
            get
            {
                return flags.IsBitSet(FlagsByteOffset.Sz);
            }

            set
            {
                Assert.False(value && LongMode, "Size type invalid for long mode");

                flags = flags.SetBit(FlagsByteOffset.Sz, value);
            }
        }

        public EAddressMode AddressMode
        {
            get
            {
                if (SizeBit)
                    return EAddressMode.Bits32;
                else
                    if (LongMode)
                    return EAddressMode.Bits64;
                else
                    return EAddressMode.Bits16;
            }

            set
            {
                if (value == EAddressMode.Bits32)
                {
                    SizeBit = true;
                    LongMode = false;
                }
                else
                    if (value == EAddressMode.Bits16)
                {
                    LongMode = false;
                    SizeBit = false;
                }
                else
                {
                    LongMode = true;
                    SizeBit = false;
                }
            }
        }

        public bool Granularity
        {
            get
            {
                return flags.IsBitSet(FlagsByteOffset.Gr);
            }

            private set
            {
                flags = flags.SetBit(FlagsByteOffset.Gr, value);
            }
        }

        public enum EAddressMode : byte
        {
            Bits16 = 16,
            Bits32 = 32,
            Bits64 = 64,
        }

        #endregion Flags

        public override string ToString()
        {
            if (IsNullDescriptor)
                return "NullDescriptor";

            var s = ""
                + "BA=" + BaseAddress.ToHex()
                + ",Limit=" + Limit.ToHex()
                + ",Ring=" + this.PriviligeRing.ToString()
                + ",Mode=" + AddressMode.ToStringNumber()
                + ",Present=" + this.Present.ToChar()
                + ",Segment=" + this.IsUserType.ToChar()
                + ",Cust=" + this.TSS_AVL.ToChar()
            ;
            string seg = "";
            if (IsUserType)
            {
                seg = ""
                    + ",Exec=" + this.UserDescriptor_Executable.ToChar()
                    + ",RW=" + ReadWrite.ToChar()
                    + ",AC=" + UserDescriptor_Accessed.ToChar()
                    + ",DC=" + this.DirectionConfirming.ToChar()
                ;
            }
            return s + seg;
        }
    }

    public struct TaskStateSegment
    {
        public const byte EntrySize = 104;

        public uint Back_link;
        public uint ESP0;
        public uint SS0;
        public uint ESP1;
        public uint SS1;
        public uint ESP2;
        public uint SS2;
        public uint CR3;
        public uint EIP;
        public uint EFLAGS;
        public uint EAX;
        public uint ECX;
        public uint EDX;
        public uint EBX;
        public uint ESP;
        public uint EBP;
        public uint ESI;
        public uint EDI;
        public uint ES;
        public uint CS;
        public uint SS;
        public uint DS;
        public uint FS;
        public uint GS;
        public uint LDT;
        public uint Trace_bitmap;
    }

}
