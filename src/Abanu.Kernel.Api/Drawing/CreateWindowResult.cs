// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

namespace Abanu.Kernel
{
    public struct CreateWindowResult
    {
        public Addr Addr;
        public int Pitch;
        public int Width;
        public int Height;
        public int Depth;
    }
}
