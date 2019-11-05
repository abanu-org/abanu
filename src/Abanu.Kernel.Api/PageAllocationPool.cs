// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

namespace Abanu.Kernel.Core
{
    public enum PageAllocationPool
    {
        /// <summary>
        /// Default Kernel pool
        /// </summary>
        Normal,

        /// <summary>
        /// Pool for Virtual Addresses equals Physical Addresses.
        /// </summary>
        Identity,

        /// <summary>
        /// Pages for Message buffers, shared between processes.
        /// </summary>
        Global,
    }

}
