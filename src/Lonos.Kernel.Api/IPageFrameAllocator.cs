// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;

namespace Lonos.Kernel.Core
{

    public enum AllocatePageOptions
    {
        Default = 0,
        Continuous = 1,
    }

    public enum AddressSpaceKind
    {
        Physical = 0,
        Virtual = 1,
    }

    public unsafe interface IPageFrameAllocator
    {
        Page* AllocatePages(uint pages);

        Page* AllocatePage();

        void Free(Page* page);

        Page* GetPhysPage(Addr addr);
    }

}
