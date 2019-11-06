// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

namespace Abanu.Kernel.Core
{
    public static class KnownSegments
    {

        private const ushort KernelRing = 0;
        private const ushort UserRing = 3;

        public const ushort NullSegment = 0x0;

        public const ushort KernelCode = (1 << 3) | KernelRing;
        public const ushort KernelData = (2 << 3) | KernelRing;

        private const ushort Reserved1 = (3 << 3) | KernelRing;
        public const ushort KernelThreadStorage = (4 << 3) | KernelRing;
        private const ushort Reserved2 = (5 << 3) | KernelRing;

        public const ushort UserCode = (6 << 3) | UserRing;
        public const ushort UserData = (7 << 3) | UserRing;
        public const ushort UserThreadStorage = (8 << 3) | UserRing;

        public const ushort KernelTSS = (9 << 3) | KernelRing;
        public const ushort UserTSS = (9 << 3) | UserRing;
    }

}
