// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System;
using System.IO;
using Abanu.Runtime;

namespace Abanu.Kernel
{

    /// <summary>
    /// General purpose Stream for /dev/null
    /// </summary>
    internal class NullStream : Stream
    {

        public override long Length => int.MaxValue;

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Position { get => 0; set => Seek(value, SeekOrigin.Current); }

        protected override void Dispose(bool disposing)
        {
        }

        public override void Flush()
        {

        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            for (var i = 0; i < count; i++)
                buffer[i] = 0;

            return count;
        }

        public override int ReadByte()
        {
            return 0;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return 0;
        }

        public override void SetLength(long value)
        {
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
        }

        public override void WriteByte(byte value)
        {
        }
    }

}
