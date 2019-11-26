// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System;
using System.IO;
using Abanu.Runtime;

namespace Abanu.Kernel
{

    /// <summary>
    /// General purpose Fifo
    /// </summary>
    internal class FifoStream : Stream
    {
        private byte[] Data;
        private int WritePosition;
        private int ReadPosition;

        private long _Length;
        public override long Length => _Length;

        public override bool CanRead => throw new NotImplementedException("NotImpelmented: CanRead");

        public override bool CanSeek => throw new NotImplementedException("NotImpelmented: CanSeek");

        public override bool CanWrite => throw new NotImplementedException("NotImpelmented: CanWrite");

        public override long Position { get => throw new NotImplementedException("NotImpelmented: GetPosition"); set => throw new NotImplementedException("NotImpelmented: SetPosition"); }

        public FifoStream(int capacity)
        {
            Data = new byte[capacity];
            _Length = 0;
        }

        protected override void Dispose(bool disposing)
        {
            RuntimeMemory.FreeObject(Data);
        }

        public override void Flush()
        {

        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_Length == 0)
                return 0;

            var cnt = (int)Math.Min(count, Length);
            for (var i = 0; i < cnt; i++)
            {
                buffer[i] = Data[ReadPosition++];
                if (ReadPosition >= Data.Length)
                    ReadPosition = 0;
                _Length--;
            }

            return cnt;
        }

        public override int ReadByte()
        {
            if (_Length == 0)
                return -1;

            var cnt = (int)Math.Min(1, Length);
            for (var i = 0; i < cnt; i++)
            {
                var result = Data[ReadPosition++];
                if (ReadPosition >= Data.Length)
                    ReadPosition = 0;
                _Length--;
                return result;
            }

            return -1;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException("NotImpelmented: seek");
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException("NotImpelmented: SetLength");
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            for (var i = 0; i < count; i++)
            {
                Data[WritePosition++] = buffer[i];
                if (WritePosition >= Data.Length)
                    WritePosition = 0;
                _Length++;
            }
        }

        public override void WriteByte(byte value)
        {
            for (var i = 0; i < 1; i++)
            {
                Data[WritePosition++] = value;
                if (WritePosition >= Data.Length)
                    WritePosition = 0;
                _Length++;
            }
        }
    }

}
