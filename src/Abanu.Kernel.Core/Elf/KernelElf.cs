// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Runtime.InteropServices;
using Abanu.Kernel.Core.Diagnostics;
using Mosa.Runtime.x86;

namespace Abanu.Kernel.Core.Elf
{

    /// <summary>
    /// Holds a reference to the Kernel's ELF sections
    /// </summary>
    public static unsafe class KernelElf
    {
        public static ElfSections Main;
        public static ElfSections Native;

        public static void Setup()
        {
            KernelMessage.WriteLine("Setup ELF Headers");
            //KernelMessage.WriteLine("Image Header:");
            //KernelMemory.DumpToConsoleLine(Address.KernelElfSection, 124);

            Main = FromAddress(Address.KernelElfSectionVirt);
            Native = FromSectionName("native");
        }

        public static unsafe ElfSections FromAddress(Addr elfStart)
        {
            var elfHeader = (ElfHeader*)elfStart;

            if (elfHeader->Ident1 != ElfHeader.Magic1)
            {
                KernelMessage.WriteLine("No valid ELF found at {0:X8}", elfStart);
                // TODO: Throw Exception
            }

            if (KConfig.Log.ELF)
                KernelMessage.WriteLine("Found ELF at {0:X8}", elfStart);

            return ElfSections.FromAddress(elfStart);
        }

        public static unsafe ElfSections FromSectionName(string name)
        {

            bool success;
            var elf = FromSectionName(name, out success);
            if (!success)
                Panic.Error("Could not find section " + name);
            return elf;
        }

        public static unsafe ElfSections FromSectionName(string name, out bool success)
        {
            var sec = Main.GetSectionHeader(name);
            if (sec == null)
            {
                success = false;
                return new ElfSections();
            }
            success = true;
            var addr = Main.GetSectionPhysAddr(sec);
            return FromAddress(addr);
        }

    }

}
