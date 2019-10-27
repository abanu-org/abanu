// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

namespace Abanu.Kernel
{
    public struct BootInfoFramebufferInfo
    {
        public Addr FbAddr;
        public uint FbPitch;
        public uint FbWidth;
        public uint FbHeight;
        public byte FbBpp;
        public byte FbType;
        public uint ColorInfo;
    }
}
