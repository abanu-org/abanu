// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using Abanu.Kernel.Core;
using Abanu.Runtime;

namespace Abanu.Kernel
{

    internal class FifoFile : IBuffer, IDisposable
    {
        private IBuffer Data;

        public FifoFile()
        {
            Data = new FifoStream(256);
        }

        public void Dispose()
        {
            RuntimeMemory.FreeObject(Data);
        }

        public unsafe SSize Read(byte* buf, USize count)
        {
            return Data.Read(buf, count);
        }

        public unsafe SSize Write(byte* buf, USize count)
        {
            return Data.Write(buf, count);
        }
    }

}
