// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using Abanu.Kernel.Core.MemoryManagement;

namespace Abanu.Kernel.Core.Collections
{
    public class FifoQueue<T> : IDisposable
    {

        public FifoQueue(int fifoSize)
        {
            FifoSize = fifoSize;
            FifoBuffer = new T[fifoSize];
        }

        protected int FifoSize;

        protected T[] FifoBuffer;

        protected uint FifoStart;

        protected uint FifoEnd;

        protected void Write(T value)
        {
            uint next = FifoEnd + 1;

            if (next == FifoSize)
                next = 0;

            if (next == FifoStart)
                return; // out of room

            FifoBuffer[next] = value;
            FifoEnd = next;
        }

        /// <summary>
        /// Gets scan code from FIFO.
        /// </summary>
        protected T Read()
        {
            if (FifoEnd == FifoStart)
                return default;   // should not happen

            T value = FifoBuffer[FifoStart];

            FifoStart++;

            if (FifoStart == FifoSize)
                FifoStart = 0;

            return value;
        }

        public void Dispose()
        {
            Memory.FreeObject(FifoBuffer);
            Memory.FreeObject(this);
        }
    }

}
