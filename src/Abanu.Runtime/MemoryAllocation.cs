// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using Abanu.Kernel.Core;
using System;

namespace Abanu.Runtime
{
    public class MemoryAllocation : IDisposable
    {
        private Addr _Start;
        public Addr Start => _Start;

        private int _Size;
        public int Size => _Size;

        internal MemoryAllocation(Addr start, int size)
        {
            _Start = start;
            _Size = size;
            _Region = new MemoryRegion(start, (uint)size);
        }

        internal MemoryAllocation(MemoryRegion region)
        {
            _Start = region.Start;
            _Size = (int)region.Size;
            _Region = region;
        }

        private MemoryRegion _Region;
        public MemoryRegion Region => _Region;

        private bool Disposed;
        public void Dispose()
        {
            if (Disposed)
                return;
            // TODO: Implement

            Disposed = true;
        }

    }

}
