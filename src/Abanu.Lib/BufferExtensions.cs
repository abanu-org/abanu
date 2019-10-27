// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using Abanu.Kernel.Core;

namespace Abanu
{
    public static class BufferExtensions
    {

        public static void Write(this IBufferWriter file, string value)
        {
            for (var i = 0; i < value.Length; i++)
            {
                file.Write(value[i]);
            }
        }

        public static unsafe void Write(this IBufferWriter file, char value)
        {
            var b = (byte)value;
            byte* ptr = &b;
            file.Write(ptr, 1);
        }

        public static unsafe void Write(this IBufferWriter file, byte value)
        {
            byte* ptr = &value;
            file.Write(ptr, 1);
        }

    }
}
