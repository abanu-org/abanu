using System;
using Mosa.Kernel.x86;

namespace lonos.kernel.core
{
    public unsafe class BootInfo_
    {
        static BootInfoHeader* BootInfo;

        public static void Setup()
        {
            BootInfo = (BootInfoHeader*)Address.KernelBootInfo;
            BootInfo->Magic = lonos.kernel.core.BootInfoHeader.BootInfoMagic;
            BootInfo->HeapStart = KMath.AlignValueCeil(Address.OriginalKernelElfSection + Start.OriginalKernelElf.TotalFileSize, 0x1000);
            BootInfo->HeapSize = 0;

            BootInfo->InstalledPhysicalMemory = 128 * 1024 * 1024;

            SetupVideoInfo();
            SetupMemoryMap();
        }

        static void SetupVideoInfo()
        {
            BootInfo->VBEPresent = Multiboot.VBEPresent;
            BootInfo->VBEMode = Multiboot.VBEMode;

            KernelMessage.WriteLine("FrameBuffer present: ", Multiboot.FBPresent ? "yes" : "no");
            if (Multiboot.FBPresent)
                KernelMessage.WriteLine("FB Present!");
            BootInfo->FBPresent = Multiboot.FBPresent;

            BootInfo->FbInfo = new BootInfoFramebufferInfo();
            BootInfo->FbInfo.FbAddr = Multiboot.multiBootInfo->FbAddr;
            BootInfo->FbInfo.FbPitch = Multiboot.multiBootInfo->FbPitch;
            BootInfo->FbInfo.FbWidth = Multiboot.multiBootInfo->FbWidth;
            BootInfo->FbInfo.FbHeight = Multiboot.multiBootInfo->FbHeight;
            BootInfo->FbInfo.FbBpp = Multiboot.multiBootInfo->FbBpp;
            BootInfo->FbInfo.FbType = Multiboot.multiBootInfo->FbType;
            BootInfo->FbInfo.ColorInfo = Multiboot.multiBootInfo->ColorInfo;
        }

        static uint MemoryMapReserve = 30;

        static void SetupMemoryMap()
        {
            uint customMaps = 7;
            var mbMapCount = Multiboot.MemoryMapCount;
            BootInfo->MemoryMapLength = mbMapCount + customMaps;
            BootInfo->MemoryMapArray = (BootInfoMemory*)MallocBootInfoData((USize)(sizeof(MultiBootMemoryMap) * MemoryMapReserve));

            for (uint i = 0; i < mbMapCount; i++)
            {
                BootInfo->MemoryMapArray[i].Start = Multiboot.GetMemoryMapBase(i);
                BootInfo->MemoryMapArray[i].Size = Multiboot.GetMemoryMapLength(i);
                var memType = BootInfoMemoryType.Reserved;
                var type = (BIOSMemoryMapType)Multiboot.GetMemoryMapType(i);

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
            }

            var idx = mbMapCount + 0;
            BootInfo->MemoryMapArray[idx].Start = Address.OriginalKernelElfSection;
            BootInfo->MemoryMapArray[idx].Size = KMath.AlignValueCeil(Start.OriginalKernelElf.TotalFileSize, 0x1000);
            BootInfo->MemoryMapArray[idx].Type = BootInfoMemoryType.OriginalKernelElfImage;

            idx++;
            BootInfo->MemoryMapArray[idx].Start = Address.KernelElfSection;
            BootInfo->MemoryMapArray[idx].Size = KMath.AlignValueCeil(Start.OriginalKernelElf.TotalFileSize, 0x1000);
            BootInfo->MemoryMapArray[idx].Type = BootInfoMemoryType.KernelElf;

            idx++;
            BootInfo->MemoryMapArray[idx].Start = Address.KernelBootInfo;
            BootInfo->MemoryMapArray[idx].Size = 0x1000;
            BootInfo->MemoryMapArray[idx].Type = BootInfoMemoryType.BootInfoHeader;

            idx++;
            BootInfo->MemoryMapArray[idx].Start = BootInfo->HeapStart;
            BootInfo->MemoryMapArray[idx].Size = 0x1000; //TODO: Recaluclate after Setup all Infos
            BootInfo->MemoryMapArray[idx].Type = BootInfoMemoryType.BootInfoHeap;

            idx++;
            uint stackSize = 0x100000; // 1MB 
            BootInfo->MemoryMapArray[idx].Start = Address.InitialStack - stackSize;
            BootInfo->MemoryMapArray[idx].Size = stackSize;
            BootInfo->MemoryMapArray[idx].Type = BootInfoMemoryType.InitialStack;

            idx++;
            BootInfo->MemoryMapArray[idx].Start = Address.GCInitialMemory;
            BootInfo->MemoryMapArray[idx].Size = 0x100000; // 1MB
            BootInfo->MemoryMapArray[idx].Type = BootInfoMemoryType.InitialGCMemory;

            idx++;
            BootInfo->MemoryMapArray[idx].Start = 0x0;
            BootInfo->MemoryMapArray[idx].Size = 0xA0000; // 640 KB
            BootInfo->MemoryMapArray[idx].Type = BootInfoMemoryType.KernelReserved;
        }

        public static void AddMap(BootInfoMemory map)
        {
            Assert.False(BootInfo->MemoryMapLength >= MemoryMapReserve);

            BootInfo->MemoryMapArray[BootInfo->MemoryMapLength] = map;
            BootInfo->MemoryMapLength++;
        }

        static Addr MallocBootInfoData(USize size)
        {
            var ret = BootInfo->HeapStart + BootInfo->HeapSize;
            BootInfo->HeapSize += size;
            return ret;
        }

    }
}
