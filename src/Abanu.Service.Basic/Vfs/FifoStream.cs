// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using Abanu.Kernel.Core;
using Abanu.Runtime;

namespace Abanu.Kernel
{

    /// <summary>
    /// General purpose Fifo
    /// </summary>
    internal class FifoStream : IBuffer, IDisposable
    {
        private byte[] Data;
        private int WritePosition;
        private int ReadPosition;
        public int Length;

        public FifoStream(int capacity)
        {
            Data = new byte[capacity];
        }

        public unsafe SSize Read(byte* buf, USize count)
        {
            if (Length == 0)
                return 0;

            var cnt = Math.Min(count, Length);
            for (var i = 0; i < cnt; i++)
            {
                buf[i] = Data[ReadPosition++];
                if (ReadPosition >= Data.Length)
                    ReadPosition = 0;
                Length--;
            }

            return cnt;
        }

        public unsafe SSize Write(byte* buf, USize count)
        {
            for (var i = 0; i < count; i++)
            {
                Data[WritePosition++] = buf[i];
                if (WritePosition >= Data.Length)
                    WritePosition = 0;
                Length++;
            }
            return (uint)count;
        }

        public void Dispose()
        {
            RuntimeMemory.FreeObject(Data);
        }
    }

}
