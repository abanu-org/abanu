// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abanu
{
    public static class StreamExtensions
    {

        //[Obsolete("Avoid")]
        public static unsafe void Write(this Stream destination, byte* buf, int count)
        {
            for (var i = 0; i < count; i++)
                destination.WriteByte(buf[i]);
        }

        //[Obsolete("Avoid")]
        public static unsafe int Read(this Stream source, byte* buf, int count)
        {
            // TODO: use array write
            for (var i = 0; i < count; i++)
            {
                var result = source.ReadByte();
                if (result >= 0)
                {
                    buf[i] = (byte)result;
                }
                else
                {
                    return i;
                }
            }
            return count;
        }

    }
}
