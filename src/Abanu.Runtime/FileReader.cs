// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using Abanu.Kernel.Core;
using Abanu.Runtime;

namespace Abanu.Kernel
{
    public class FileReader : IBufferReader
    {

        private FileHandle Handle;

        public FileReader(FileHandle handle)
        {
            Handle = handle;
        }

        public unsafe SSize Read(byte* buf, USize count)
        {
            return SysCalls.ReadFile(Handle, new MemoryRegion(buf, count));
        }

    }

}
