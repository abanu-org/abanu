// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;

namespace Lonos.Kernel.Core.Boot
{
    public struct BootInfoMemory
    {
        public Addr Start;
        public USize Size;
        public BootInfoMemoryType Type;

        /// <summary>
        /// Specifies the address space kind of <see cref="Start"/>
        /// </summary>
        public AddressSpaceKind AddressSpaceKind;
        //public bool CanWrite;
        //public bool CanExecute;

        public bool PreMap;

    }

}
