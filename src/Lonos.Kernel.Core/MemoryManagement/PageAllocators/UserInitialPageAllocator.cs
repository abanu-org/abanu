// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using Lonos.Kernel.Core.Boot;
using Lonos.Kernel.Core.Devices;
using Lonos.Kernel.Core.Diagnostics;
using Lonos.Kernel.Core.PageManagement;
using Mosa.Runtime;

//using Mosa.Kernel.x86;

namespace Lonos.Kernel.Core.MemoryManagement.PageAllocators
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
