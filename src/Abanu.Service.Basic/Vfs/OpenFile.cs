// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using Abanu.Kernel.Core;

namespace Abanu.Kernel
{

    internal class OpenFile
    {
        public FileHandle Handle;
        public string Path;
        public int ProcessId;
        public IBuffer Buffer;
    }

}
