// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System;
using System.IO;
using Abanu.Runtime;

#pragma warning disable CA1812 // Avoid uninstantiated internal classes

namespace Abanu.Kernel
{

    /// <summary>
    /// Read will wait for at least one byte
    /// </summary>
    internal class BlockingStream : Stream
    {
        private Stream BaseStream;

        public BlockingStream(Stream baseStream)
        {
            BaseStream = baseStream;
        }

        public override bool CanRead => BaseStream.CanRead;

        public override bool CanSeek => BaseStream.CanSeek;

        public override bool CanWrite => BaseStream.CanWrite;

        public override long Length => BaseStream.Length;

        public override long Position { get => BaseStream.Position; set => BaseStream.Position = value; }

        public override void Flush()
        {
            BaseStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int cnt = 0;
            while (cnt == 0) // TODO: Suspend Thread
                cnt = BaseStream.Read(buffer, offset, count);
            return cnt;
        }

        public override int ReadByte()
        {
            var result = -1;
            while (result == -1) // TODO: Suspend Thread
                result = BaseStream.ReadByte();
            return result;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return BaseStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            BaseStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            BaseStream.Write(buffer, offset, count);
        }

        public override void WriteByte(byte value)
        {
            BaseStream.WriteByte(value);
        }
    }

}
