// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using Lonos.Kernel.Core;

namespace Lonos.Kernel
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
