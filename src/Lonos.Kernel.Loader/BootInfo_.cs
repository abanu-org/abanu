// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using Lonos.Kernel.Core;
using Lonos.Kernel.Core.Boot;
using Lonos.Kernel.Core.PageManagement;

namespace Lonos.Kernel.Loader
{
    public static unsafe class BootInfo_
    {
        public static BootInfoHeader* BootInfo;

        public static void Setup()
        {
            KernelMessage.WriteLine("Multiboot Flags: {0:X}", Multiboot.Flags);
            BootInfo = (BootInfoHeader*)Address.KernelBootInfo;
            BootInfo->Magic = BootInfoHeader.BootInfoMagic;
            BootInfo->HeapStart = KMath.AlignValueCeil(Address.OriginalKernelElfSection + LoaderStart.OriginalKernelElf.TotalFileSize, 0x1000);
            BootInfo->HeapSize = 0;

            BootInfo->InstalledPhysicalMemory = 256 * 1024 * 1024;
            BootInfo->PageTableType = KConfig.UsePAE ? PageTableType.PAE : PageTableType.X86;
            BootInfo->KernelBootStartCycles = PerformanceCounter.KernelBootStartCycles;

            SetupVideoInfo();
            SetupMemoryMap();
        }

        private static void SetupVideoInfo()
        {
            KernelMessage.WriteLine("VBE present: {0}", Multiboot.VBEPresent ? "yes" : "no");
            if (Multiboot.VBEPresent)
                KernelMessage.WriteLine("VBE Mode: {0}", Multiboot.VBEMode);
            BootInfo->VBEPresent = Multiboot.VBEPresent;
            BootInfo->VBEMode = Multiboot.VBEMode;

            KernelMessage.WriteLine("FrameBuffer present: {0}", Multiboot.FBPresent ? "yes" : "no");
            BootInfo->FBPresent = Multiboot.FBPresent;

            BootInfo->FbInfo = new BootInfoFramebufferInfo
            {
                FbAddr = Multiboot.MultiBootInfo->FbAddr,
                FbPitch = Multiboot.MultiBootInfo->FbPitch,
                FbWidth = Multiboot.MultiBootInfo->FbWidth,
                FbHeight = Multiboot.MultiBootInfo->FbHeight,
                FbBpp = Multiboot.MultiBootInfo->FbBpp,
                FbType = Multiboot.MultiBootInfo->FbType,
                ColorInfo = Multiboot.MultiBootInfo->ColorInfo,
            };
        }

        private const uint MemoryMapReserve = 40;

        private static void SetupMemoryMap()
        {
            var mbMapCount = Multiboot.MemoryMapCount;
            BootInfo->MemoryMapArray = (BootInfoMemory*)MallocBootInfoData((USize)(sizeof(MultiBootMemoryMap) * MemoryMapReserve));

            for (uint i = 0; i < mbMapCount; i++)
            {
                BootInfo->MemoryMapArray[i].Start = Multiboot.GetMemoryMapBase(i);
                BootInfo->MemoryMapArray[i].Size = Multiboot.GetMemoryMapLength(i);
                var memType = BootInfoMemoryType.Reserved;
                var type = (BIOSMemoryMapType)Multiboot.GetMemoryMapType(i);
                var addressSpaceKind = AddressSpaceKind.Physical;
                var preMap = false;

                switch (type)
                {
                    case BIOSMemoryMapType.Usable:
                        memType = BootInfoMemoryType.SystemUsable;
                        break;
                    case BIOSMemoryMapType.Reserved:
                        memType = BootInfoMemoryType.Reserved;
                        break;
                    case BIOSMemoryMapType.ACPI_Relaimable:
                        memType = BootInfoMemoryType.ACPI_Relaimable;
                        break;
                    case BIOSMemoryMapType.ACPI_NVS_Memory:
                        memType = BootInfoMemoryType.ACPI_NVS_Memory;
                        break;
                    case BIOSMemoryMapType.BadMemory:
                        memType = BootInfoMemoryType.BadMemory;
                        break;
                    default:
                        memType = BootInfoMemoryType.Unknown;
                        break;
                }
                BootInfo->MemoryMapArray[i].Type = memType;
                BootInfo->MemoryMapArray[i].AddressSpaceKind = addressSpaceKind;
                BootInfo->MemoryMapArray[i].PreMap = preMap;
            }

            // It's possible, that the BIOS-Area (@640K) is not correctly setup by Multiboot. So add it here manually to be sure
            var idx = mbMapCount + 0;
            BootInfo->MemoryMapArray[idx].Start = 640 * 1024;
            BootInfo->MemoryMapArray[idx].Size = (1024 - 640) * 1024;
            BootInfo->MemoryMapArray[idx].Type = BootInfoMemoryType.CustomReserved;
            BootInfo->MemoryMapArray[idx].AddressSpaceKind = AddressSpaceKind.Both; //TODO: Physical
            BootInfo->MemoryMapArray[idx].PreMap = true;

            idx++;
            BootInfo->MemoryMapArray[idx].Start = Address.KernelElfSectionPhys;
            BootInfo->MemoryMapArray[idx].Size = KMath.AlignValueCeil(LoaderStart.OriginalKernelElf.TotalFileSize, 0x1000);
            BootInfo->MemoryMapArray[idx].Type = BootInfoMemoryType.KernelElf;
            BootInfo->MemoryMapArray[idx].AddressSpaceKind = AddressSpaceKind.Physical;
            BootInfo->MemoryMapArray[idx].PreMap = true;

            idx++;
            BootInfo->MemoryMapArray[idx].Start = Address.KernelBootInfo;
            BootInfo->MemoryMapArray[idx].Size = 0x1000;
            BootInfo->MemoryMapArray[idx].Type = BootInfoMemoryType.BootInfoHeader;
            BootInfo->MemoryMapArray[idx].AddressSpaceKind = AddressSpaceKind.Both;
            BootInfo->MemoryMapArray[idx].PreMap = true;

            idx++;
            BootInfo->MemoryMapArray[idx].Start = BootInfo->HeapStart;
            BootInfo->MemoryMapArray[idx].Size = 0x1000; //TODO: Recalculate after Setup all Infos
            BootInfo->MemoryMapArray[idx].Type = BootInfoMemoryType.BootInfoHeap;
            BootInfo->MemoryMapArray[idx].AddressSpaceKind = AddressSpaceKind.Both;
            BootInfo->MemoryMapArray[idx].PreMap = true;

            idx++;
            uint stackSize = 0x100000; // 1MB
            BootInfo->MemoryMapArray[idx].Start = Address.InitialStack - stackSize;
            BootInfo->MemoryMapArray[idx].Size = stackSize;
            BootInfo->MemoryMapArray[idx].Type = BootInfoMemoryType.InitialStack;
            BootInfo->MemoryMapArray[idx].AddressSpaceKind = AddressSpaceKind.Both;
            BootInfo->MemoryMapArray[idx].PreMap = true;

            idx++;
            BootInfo->MemoryMapArray[idx].Start = Address.GCInitialMemory;
            BootInfo->MemoryMapArray[idx].Size = Address.GCInitialMemorySize;
            BootInfo->MemoryMapArray[idx].Type = BootInfoMemoryType.InitialGCMemory;
            BootInfo->MemoryMapArray[idx].AddressSpaceKind = AddressSpaceKind.Both;
            BootInfo->MemoryMapArray[idx].PreMap = true;

            idx++;
            BootInfo->MemoryMapArray[idx].Start = LoaderStart.OriginalKernelElf.GetSectionHeader(".bss")->Addr;
            BootInfo->MemoryMapArray[idx].Size = LoaderStart.OriginalKernelElf.GetSectionHeader(".bss")->Size;
            BootInfo->MemoryMapArray[idx].Type = BootInfoMemoryType.KernelBssSegment;
            BootInfo->MemoryMapArray[idx].AddressSpaceKind = AddressSpaceKind.Virtual;
            BootInfo->MemoryMapArray[idx].PreMap = false;

            idx++;
            BootInfo->MemoryMapArray[idx].Start = LoaderStart.OriginalKernelElf.GetSectionHeader(".text")->Addr;
            BootInfo->MemoryMapArray[idx].Size = LoaderStart.OriginalKernelElf.GetSectionHeader(".text")->Size;
            BootInfo->MemoryMapArray[idx].Type = BootInfoMemoryType.KernelTextSegment;
            BootInfo->MemoryMapArray[idx].AddressSpaceKind = AddressSpaceKind.Virtual;
            BootInfo->MemoryMapArray[idx].PreMap = false;

            idx++;
            BootInfo->MemoryMapArray[idx].Start = LoaderStart.OriginalKernelElf.GetSectionHeader(".rodata")->Addr;
            BootInfo->MemoryMapArray[idx].Size = LoaderStart.OriginalKernelElf.GetSectionHeader(".rodata")->Size;
            BootInfo->MemoryMapArray[idx].Type = BootInfoMemoryType.KernelROdataSegment;
            BootInfo->MemoryMapArray[idx].AddressSpaceKind = AddressSpaceKind.Virtual;
            BootInfo->MemoryMapArray[idx].PreMap = false;

            idx++;
            BootInfo->MemoryMapArray[idx].Start = LoaderStart.OriginalKernelElf.GetSectionHeader(".data")->Addr;
            BootInfo->MemoryMapArray[idx].Size = LoaderStart.OriginalKernelElf.GetSectionHeader(".data")->Size;
            BootInfo->MemoryMapArray[idx].Type = BootInfoMemoryType.KernelDataSegment;
            BootInfo->MemoryMapArray[idx].AddressSpaceKind = AddressSpaceKind.Virtual;
            BootInfo->MemoryMapArray[idx].PreMap = false;

            // Avoiding the use of the first megabyte of RAM
            idx++;
            BootInfo->MemoryMapArray[idx].Start = 0x0;
            BootInfo->MemoryMapArray[idx].Size = Address.ReserveMemory;
            BootInfo->MemoryMapArray[idx].Type = BootInfoMemoryType.KernelReserved;
            BootInfo->MemoryMapArray[idx].AddressSpaceKind = AddressSpaceKind.Both;
            BootInfo->MemoryMapArray[idx].PreMap = false;

            BootInfo->MemoryMapLength = idx + 1;
        }

        public static void AddMap(BootInfoMemory map)
        {
            Assert.False(BootInfo->MemoryMapLength >= MemoryMapReserve);

            BootInfo->MemoryMapArray[BootInfo->MemoryMapLength] = map;
            BootInfo->MemoryMapLength++;
        }

        private static Addr MallocBootInfoData(USize size)
        {
            var ret = BootInfo->HeapStart + BootInfo->HeapSize;
            BootInfo->HeapSize += size;
            return ret;
        }

    }
}
