// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System;
using Abanu.Kernel.Core.Boot;
using Abanu.Kernel.Core.Devices;
using Abanu.Kernel.Core.Diagnostics;
using Abanu.Kernel.Core.PageManagement;
using Mosa.Runtime;

//using Mosa.Kernel.x86;

namespace Abanu.Kernel.Core.MemoryManagement.PageAllocators
{

    public unsafe class UserInitialPageAllocator : InitialPageAllocator2
    {

        public UserInitialPageAllocator()
        {
        }

        protected override MemoryRegion AllocRawMemory(uint size)
        {
            var kmap = VirtualPageManager.AllocateRegion(size);
            kmap.Clear();
            return kmap;
        }

        /// <summary>
        /// Setups the free memory.
        /// </summary>
        protected override unsafe void SetupFreeMemory()
        {
            for (uint i = 0; i < _TotalPages; i++)
                PageArray[i].Status = PageStatus.Free;
        }

    }

}
