// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using Abanu.Kernel.Core;
using System;

namespace Abanu.Runtime
{
    public class MemoryAllocation : IDisposable
    {
        private unsafe byte* BytePtr;

        private Addr _Start;
        public Addr Start => _Start;

        private int _Size;
        public int Size => _Size;

        internal unsafe MemoryAllocation(Addr start, int size)
        {
            BytePtr = (byte*)start;
            _Start = start;
            _Size = size;
            _Region = new MemoryRegion(start, (uint)size);
        }

        internal unsafe MemoryAllocation(MemoryRegion region)
        {
            BytePtr = (byte*)region.Start;
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

        public unsafe byte GetByte(int offset)
        {
            if (offset >= _Size)
                throw new ArgumentOutOfRangeException(nameof(offset));

            return BytePtr[offset];
        }

        public unsafe void SetByte(int offset, byte value)
        {
            if (offset >= _Size)
                throw new ArgumentOutOfRangeException(nameof(offset));

            BytePtr[offset] = value;
        }

        public unsafe void Write(byte[] sourceArray, int sourceIndex, int destinationIndex, int count)
        {
            if (destinationIndex + count > _Size)
                throw new ArgumentOutOfRangeException();

            if (sourceIndex + count > sourceArray.Length)
                throw new ArgumentOutOfRangeException();

            for (var i = 0; i < count; i++)
                BytePtr[destinationIndex + i] = sourceArray[sourceIndex + i];
        }

        public unsafe void Read(int sourceIndex, byte[] destinationArray, int destinationIndex, int count)
        {
            if (sourceIndex + count > _Size)
                throw new ArgumentOutOfRangeException();

            if (destinationIndex + count > destinationArray.Length)
                throw new ArgumentOutOfRangeException();

            for (var i = 0; i < count; i++)
                destinationArray[destinationIndex + i] = BytePtr[sourceIndex + i];
        }

    }

}
