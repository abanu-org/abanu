// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Abanu.Kernel;
using Abanu.Kernel.Core;
using Abanu.Runtime;
using Mosa.Runtime.x86;

namespace System.IO
{

    public class FileStream : Stream
    {

        private FileHandle Handle;

        private MemoryAllocation ReadBuffer;
        private MemoryAllocation WriteBuffer;

        internal FileStream(FileHandle handle)
        {
            Handle = handle;
            // TODO: Store Target in FileHandle
            var targetProcessId = SysCalls.GetProcessIDForCommand(SysCallTarget.OpenFile);
            ReadBuffer = ApplicationRuntime.RequestMessageBuffer(4096, targetProcessId);
            WriteBuffer = ApplicationRuntime.RequestMessageBuffer(4096, targetProcessId);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // TODO: Free Memory
            }
        }

        public override bool CanRead => throw new NotSupportedException();

        public override bool CanSeek => throw new NotSupportedException();

        public override bool CanWrite => throw new NotSupportedException();

        public override long Length => throw new NotSupportedException();

        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public override void Flush()
        {
        }

        public override unsafe int Read(byte[] buffer, int offset, int count)
        {
            if (count > ReadBuffer.Size)
                count = (int)ReadBuffer.Size;

            if (offset + count > buffer.Length)
                count = buffer.Length - offset;

            var buf = (byte*)ReadBuffer.Start;
            var gotBytes = SysCalls.ReadFile(Handle, ReadBuffer.Region);
            for (var i = 0; i < gotBytes; i++)
                buffer[i] = buf[i];

            return gotBytes;
        }

        public override unsafe int ReadByte()
        {
            var buf = (byte*)ReadBuffer.Start;
            var gotBytes = SysCalls.ReadFile(Handle, new MemoryRegion(buf, 1));
            if (gotBytes >= 1)
            {
                return buf[0];
            }
            else
                return -1;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override unsafe void Write(byte[] buffer, int offset, int count)
        {
            if (count > WriteBuffer.Size)
                count = (int)WriteBuffer.Size;

            if (offset + count > buffer.Length)
                count = buffer.Length - offset;

            var buf = (byte*)WriteBuffer.Start;

            for (var i = 0; i < count; i++)
                buf[i] = buffer[i + offset];

            SysCalls.WriteFile(Handle, WriteBuffer.Region);
        }

        public override unsafe void WriteByte(byte value)
        {
            var buf = (byte*)WriteBuffer.Start;
            buf[0] = value;
            var writtenBytes = SysCalls.WriteFile(Handle, new MemoryRegion(buf, 1));
        }
    }

}
