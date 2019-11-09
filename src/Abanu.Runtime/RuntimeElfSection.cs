// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Abanu;
using Abanu.Kernel.Core;
using Abanu.Kernel.Core.Elf;
using Abanu.Runtime;

#pragma warning disable CA1001 // IDisposable

namespace Abanu.Kernel
{
    public class RuntimeElfSection
    {
        private string _Name;
        public string Name => _Name;

        private unsafe ElfSectionHeader* Header;

        internal unsafe RuntimeElfSection(string name, ElfSectionHeader* header, Addr physAddr)
        {
            _Name = name;
            Header = header;
            _Data = new MemoryAllocation(physAddr, (int)header->Size)
            {
                Disposable = false,
            };
        }

        public MemoryAllocation _Data;
        public MemoryAllocation Data => _Data;

    }
}
