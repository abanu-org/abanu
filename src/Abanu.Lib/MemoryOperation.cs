// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System;
using Mosa.Runtime.x86;

namespace Abanu.Kernel.Core
{
    public static class MemoryOperation
    {

        public static void Copy(Addr source, Addr destination, USize length)
        {
            if (length > 100)
            {
                if (source % 4 == 0 && destination % 4 == 0 && length % 4 == 0)
                {
                    Copy4(source, destination, length);
                    return;
                }
            }

            for (uint i = 0; i < length; i++)
                Native.Set8(destination + i, Native.Get8(source + i));  //TODO: Optimize with Set32
        }

        public static void Copy4(Addr source, Addr destination, USize length)
        {
            var count = length / 4; //TODO: Check modulo 4 == 0
            for (uint i = 0; i < count; i += 4)
                Native.Set32(destination + i, Native.Get32(source + i));
        }

        public static unsafe void Copy4(uint source, uint destination, uint length)
        {
            var count = length / 4; //TODO: Check modulo 4 == 0
            //for (uint i = 0; i < count; i += 4)
            //{
            //    Native.Set32(destination + i, Native.Get32(source + i));
            //}

            var src = (uint*)source;
            var dst = (uint*)destination;
            for (var i = 0; i < count; i++)
            {
                dst[i] = src[i];
            }
        }

        /// <summary>
        /// Clears the specified memory area.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="bytes">The bytes.</param>
        public static void Clear(Addr start, USize bytes)
        {
            if (bytes > 100)
            {
                if (start % 4 == 0 && bytes % 4 == 0)
                {
                    Clear4(start, bytes);
                    return;
                }
            }

            for (uint at = start; at < (start + bytes); at++)
                Native.Set8(at, 0);
        }

        public static void Clear4(Addr start, USize bytes)
        {
            for (uint at = start; at < (start + bytes); at += 4)
                Native.Set32(at, 0);
        }

    }
}
