// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

#pragma warning disable

namespace Abanu.Kernel.Core
{

    public interface IUnixKernelApi
    {

        /// <summary>
        /// kmalloc is the normal method of allocating memory for objects smaller than page size in the kernel.
        /// </summary>
        Addr kmalloc(USize n, GFP flags);

        /// <summary>
        /// allocate memory. The memory is set to zero.
        /// </summary>
        Addr kzalloc(USize n, GFP flags);

        /// <summary>
        /// allocate memory for an array.
        /// </summary>
        Addr kmalloc_array(USize elements, USize size, GFP flags);

        /// <summary>
        /// allocate memory for an array. The memory is set to zero.
        /// </summary>
        Addr kcalloc(USize elements, USize size, GFP flags);

        /// <summary>
        ///
        /// </summary>
        Addr vmalloc(USize size, GFP flags);

        /// <summary>
        /// free previously allocated memory
        /// </summary>
        void kfree(Addr address);

        /// <summary>
        /// release memory allocated by vmalloc()
        /// </summary>
        void vfree(Addr address);

    }

}
