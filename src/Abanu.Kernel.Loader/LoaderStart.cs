// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System;
using Abanu.Kernel.Core;
using Abanu.Kernel.Core.Boot;
using Abanu.Kernel.Core.Elf;
using Abanu.Kernel.Core.PageManagement;
using Mosa.Runtime;
using Mosa.Runtime.Plug;
using Mosa.Runtime.x86;

namespace Abanu.Kernel.Loader
{

    public static unsafe class LoaderStart
    {

        public static void Main()
        {
            //Screen.BackgroundColor = ScreenColor.Green;
            //Screen.Color = ScreenColor.Red;
            //Screen.Clear();
            //Screen.Goto(0, 0);
            //Screen.Write('A');

            //Serial.SetupPort(Serial.COM1);
            //Serial.Write(Serial.COM1, "Hello");

            BootMemory.Setup();

            // Setup Kernel Log
            var kmsgHandler = new KernelMessageWriter();
            KernelMessage.SetHandler(kmsgHandler);
            KernelMessage.WriteLine("<LOADER:CONSOLE:BEGIN>");
            Assert.Setup(AssertError);
            PerformanceCounter.Setup();

            // Parse Boot Informations
            Multiboot.Setup();

            // Parse Kernel ELF section
            SetupOriginalKernelElf();

            // Print all section of Kernel ELF (for information only)
            DumpElfInfo();

            // Copy Section to a final destination
            SetupKernelSection();

            // Collection informations we need to pass to the kernel
            BootInfo_.Setup();

            // Setup Global Descriptor Table
            var map = BootMemory.AllocateMemoryMap(0x1000, BootInfoMemoryType.GDT, AddressSpaceKind.Both);
            BootInfo_.AddMap(map);
            GDT.Setup(map.Start);

            // Now we enable Paging. It's important that we do not cause a Page Fault Exception,
            // Because IDT is not setup yet, that could handle this kind of exception.

            PageTable.ConfigureType(BootInfo_.BootInfo->PageTableType);
            map = BootMemory.AllocateMemoryMap(PageTable.KernelTable.InitalMemoryAllocationSize, BootInfoMemoryType.PageTable, AddressSpaceKind.Both);
            BootInfo_.AddMap(map);
            PageTable.KernelTable.Setup(map.Start);
            MapMemory();
            PageTable.KernelTable.EnablePaging();

            // Because Kernel is compiled in virtual address space, we need to remap the pages
            MapKernelImage();

            // Get Entry Point of Kernel
            uint kernelEntry = GetKernelStartAddr();
            if (kernelEntry == 0)
            {
                KernelMessage.WriteLine("No kernel entry point found {0:X8}");
                KernelMessage.WriteLine("Is the name of entry point correct?");
                KernelMessage.WriteLine("Are symbols emitted?");
                KernelMessage.WriteLine("System halt!");
                while (true)
                {
                    Native.Nop();
                }
            }
            KernelMessage.WriteLine("Call Kernel Start at {0:X8}", kernelEntry);

            // Start Kernel.
            CallAddress(kernelEntry);

            // If we hit this code location, the Kernel Main method returned.
            // This would be a general fault. Normally, this code section will overwritten
            // by the kernel, so normally, it can never reach this code position.
            KernelMessage.WriteLine("Unexpected return from Kernel Start");

            Debug.Break();
        }

        private static void MapMemory()
        {
            for (var mapIdx = 0; mapIdx < BootInfo_.BootInfo->MemoryMapLength; mapIdx++)
            {
                var map = BootInfo_.BootInfo->MemoryMapArray[mapIdx];
                KernelMessage.WriteLine("Test map Type={0:X8} Start={1:X8}", (uint)map.Type, map.Start);
                if (map.PreMap && (map.AddressSpaceKind & AddressSpaceKind.Physical) != 0)
                {
                    KernelMessage.WriteLine("Map!");
                    PageTable.KernelTable.Map(map.Start, map.Start, map.Size);
                }
            }

            PageTable.KernelTable.Map(Address.LoaderBasePhys, Address.LoaderBasePhys, Address.LoaderSize);
            PageTable.KernelTable.Map(BootInfo_.BootInfo->HeapStart, BootInfo_.BootInfo->HeapStart, BootInfo_.BootInfo->HeapSize);
            PageTable.KernelTable.Map(Address.OriginalKernelElfSection, Address.OriginalKernelElfSection, Address.OriginalKernelElfSize);
            PageTable.KernelTable.Flush();
        }

        private static void MapKernelImage()
        {
            var phys = Address.KernelElfSectionPhys;
            var diff = Address.KernelBaseVirt - Address.KernelBasePhys;
            var endPhys = phys + OriginalKernelElf.TotalFileSize;
            var addr = phys;
            KernelMessage.WriteLine("Mapping Kernel Image from physical {0:X8} to virtual {1:X8}", phys, phys + diff);
            while (addr < endPhys)
            {
                PageTable.KernelTable.MapVirtualAddressToPhysical(addr + diff, addr);
                addr += 0x1000;
            }
            PageTable.KernelTable.Flush();

            var map = new BootInfoMemory
            {
                Start = phys + diff,
                Size = OriginalKernelElf.TotalFileSize,
                Type = BootInfoMemoryType.KernelElfVirt,
                AddressSpaceKind = AddressSpaceKind.Virtual,
            };
            BootInfo_.AddMap(map);
        }

        private static Addr GetKernelStartAddr()
        {
            var symName = KConfig.KernelEntryName;
            var sym = OriginalKernelElf.GetSymbol(symName);
            if (sym == (ElfSymbol*)0)
                return Addr.Zero;
            return sym->Value;
        }

        private static void CallAddress(uint addr)
        {
            Native.Call(addr);
        }

        public static ElfSections OriginalKernelElf;

        private static void SetupOriginalKernelElf()
        {
            uint kernelElfHeaderAddr = Address.OriginalKernelElfSection;
            var kernelElfHeader = (ElfHeader*)kernelElfHeaderAddr;
            OriginalKernelElf = new ElfSections
            {
                PhyOffset = kernelElfHeaderAddr,
                SectionHeaderArray = (ElfSectionHeader*)(kernelElfHeaderAddr + kernelElfHeader->ShOff),
                SectionHeaderCount = kernelElfHeader->ShNum,
                StringTableSectionHeaderIndex = kernelElfHeader->ShStrNdx,
            };
            OriginalKernelElf.Init();
        }

        private static unsafe void SetupKernelSection()
        {
            // TODO: Respect section program header address.
            // Currently, we can make a raw copy of ELF file
            MemoryOperation.Copy4(Address.OriginalKernelElfSection, Address.KernelElfSectionPhys, OriginalKernelElf.TotalFileSize);
        }

        public static unsafe void DumpElfInfo()
        {
            var secLength = OriginalKernelElf.SectionHeaderCount;

            KernelMessage.WriteLine("Found {0} Kernel Sections:", secLength);

            for (uint i = 0; i < secLength; i++)
            {
                var sec = OriginalKernelElf.GetSectionHeader(i);
                var name = OriginalKernelElf.GeSectionName(sec);
                var sb = new StringBuffer(name);
                KernelMessage.WriteLine(sb);
            }
        }

        private static void Dummy()
        {
            //This is a dummy call, that get never executed.
            //Its required, because we need a real reference to Mosa.Runtime.x86
            //Without that, the .NET compiler will optimize that reference away
            //if its nowhere used. Than the Compiler doesn't know about that reference
            //and the Compilation will fail
            Mosa.Runtime.x86.Internal.ExceptionHandler();
        }

        private static void AssertError(string message, uint arg1 = 0, uint arg2 = 0, uint arg3 = 0)
        {
            KernelMessage.WriteLine("ASSERT ERROR! ");
            var sb = new StringBuffer();
            sb.Append(message, arg1, arg2, arg3);
            KernelMessage.WriteLine(sb);
        }

    }
}
