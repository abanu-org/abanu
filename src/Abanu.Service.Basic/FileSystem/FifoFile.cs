// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System.IO;

namespace Abanu.Kernel
{
    internal class FifoFile : Stream
    {
        private Stream Data;

        public FifoFile()
        {
            //Data = new BlockingStream(new FifoStream(256));
            Data = new FifoStream(256);
        }

        public override bool CanRead => Data.CanRead;

        public override bool CanSeek => Data.CanSeek;

        public override bool CanWrite => Data.CanWrite;

        public override long Length => Data.Length;

        public override long Position { get => Data.Position; set => Data.Position = value; }

        public override void Flush()
        {
            Data.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return Data.Read(buffer, offset, count);
        }

        public override int ReadByte()
        {
            return Data.ReadByte();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return Data.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            Data.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Data.Write(buffer, offset, count);
        }

        public override void WriteByte(byte value)
        {
            Data.WriteByte(value);
        }
    }

}
