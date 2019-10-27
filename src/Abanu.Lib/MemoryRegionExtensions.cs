// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using Abanu.Kernel.Core;

namespace Abanu.Kernel
{
    public static class MemoryRegionExtensions
    {

        public static void Clear(this MemoryRegion region)
        {
            MemoryOperation.Clear(region.Start, region.Size);
        }

        public static unsafe int Checksum(this MemoryRegion region)
        {
            return FNVHash.ComputeInt32((byte*)region.Start, (int)region.Size);
        }

    }

}
