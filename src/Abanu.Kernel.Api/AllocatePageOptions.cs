// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

namespace Abanu.Kernel.Core
{
    public struct AllocatePageOptions
    {
        /// <summary>
        /// if true, the underlining physical memory must be continuous
        /// </summary>
        public bool Continuous;

        /// <summary>
        /// This is only used for debugging
        /// </summary>
        public string DebugName;

        public PageAllocationPool Pool;

        //public IPageTable Target1;
        //public IPageTable Target2;

        public static AllocatePageOptions Default;

    }

}
