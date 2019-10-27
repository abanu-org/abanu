// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

namespace Abanu.Kernel
{

    public enum GFP
    {
        ATOMIC,
        KERNEL,
        KERNEL_ACCOUNT,
        NOWAIT,
        NOIO,
        NOFS,
        USER,
        DMA,
        DMA32,
        HIGHUSER,
        HIGHUSER_MOVABLE,
        TRANSHUGE_LIGHT,
        TRANSHUGE,
        MOVABLE_MASK,
        MOVABLE_SHIFT,
    }

}
