// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Abanu.Kernel.Core;
using Abanu.Runtime;
using Mosa.Runtime.x86;

namespace Abanu.Kernel
{

    public class FileStream : IBuffer
    {

        private FileHandle Handle;

        public FileStream(FileHandle handle)
        {
            Handle = handle;
        }

        public unsafe SSize Read(byte* buf, USize count)
        {
            return SysCalls.ReadFile(Handle, new MemoryRegion(buf, count));
        }

        public unsafe SSize Write(byte* buf, USize count)
        {
            return SysCalls.WriteFile(Handle, new MemoryRegion(buf, count));
        }

    }

}
