// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using Abanu.Kernel.Core;
using Abanu.Runtime;

namespace Abanu.Kernel
{
    public class FileWriter : IBufferWriter
    {

        private FileHandle Handle;

        public FileWriter(FileHandle handle)
        {
            Handle = handle;
        }

        public unsafe SSize Write(byte* buf, USize count)
        {
            return SysCalls.WriteFile(Handle, new MemoryRegion(buf, count));
        }

    }

}
