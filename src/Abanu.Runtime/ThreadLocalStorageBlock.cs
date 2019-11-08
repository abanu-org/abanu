// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System.Runtime.InteropServices;
using System.Threading;

namespace Abanu.Runtime
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ThreadLocalStorageBlock
    {
        public uint ThreadPtr;
        public int ThreadID;
    }

}
