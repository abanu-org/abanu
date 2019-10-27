// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;
using Abanu.Kernel.Core;

namespace Abanu
{

    public static class KMath
    {

        static KMath()
        {
            Init();
        }

        public static void Init()
        {
            // Already initialized
            if (MultiplyDeBruijnBitPosition != null)
                return;

            MultiplyDeBruijnBitPosition = new uint[]
            {
                0, 9, 1, 10, 13, 21, 2, 29, 11, 14, 16, 18, 22, 25, 3, 30,
                8, 12, 20, 28, 15, 17, 24, 7, 19, 27, 23, 6, 26, 5, 4, 31,
            };

            MultiplyDeBruijnBitPosition2 = new uint[]
            {
                0, 1, 28, 2, 29, 14, 24, 3, 30, 22, 20, 15, 25, 17, 4, 8,
                31, 27, 13, 23, 21, 19, 16, 7, 26, 12, 18, 6, 11, 5, 10, 9,
            };

            Test();
        }

        /// <summary>
        /// compute the next highest power of 2
        /// See: https://graphics.stanford.edu/~seander/bithacks.html#RoundUpPowerOf2
        /// </summary>
        /// <remarks>
        /// In 12 operations, this code computes the next highest power of 2 for a 32-bit integer.
        /// The result may be expressed by the formula 1U << (lg(v - 1) + 1). Note that in the edge case where v is 0, it returns 0, which isn't a power of 2;
        /// you might append the expression v += (v == 0) to remedy this if it matters. It would be faster by 2 operations to use the formula and the
        /// log base 2 method that uses a lookup table, but in some situations, lookup tables are not suitable, so the above code may be best.
        /// (On a Athlon™ XP 2100+ I've found the above shift-left and then OR code is as fast as using a single BSR assembly language instruction,
        /// which scans in reverse to find the highest set bit.) It works by copying the highest set bit to all of the lower bits, and then adding one,
        /// which results in carries that set all of the lower bits to 0 and one bit beyond the highest set bit to 1.
        /// If the original number was a power of 2, then the decrement will reduce it to one less, so that we round up to the same original value.
        /// You might alternatively compute the next higher power of 2 in only 8 or 9 operations using a lookup table for floor(lg(v)) and then evaluating
        /// 1<<(1+floor(lg(v)));
        /// </remarks>
        public static uint CeilToPowerOfTwo(uint value)
        {
            value--;
            value |= value >> 1;
            value |= value >> 2;
            value |= value >> 4;
            value |= value >> 8;
            value |= value >> 16;
            return ++value;
        }

        // Verify!! Not Tested!!
        public static uint FloorToPowerOfTwo(uint value)
        {
            var newValue = CeilToPowerOfTwo(value);
            if (newValue == value)
                return value;
            else
                return newValue >> 1;
        }

        private static uint[] MultiplyDeBruijnBitPosition;

        /// <summary>
        /// Find the log base 2 of an N-bit integer in O(lg(N)) operations.
        /// </summary>
        /// <remarks>
        /// http://graphics.stanford.edu/~seander/bithacks.html#IntegerLogDeBruijn
        /// </remarks>
        public static uint Log2(uint v)
        {
            v |= v >> 1; // first round down to one less than a power of 2
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;

            return MultiplyDeBruijnBitPosition[(uint)(v * 0x07C4ACDDU) >> 27];
        }

        private static uint[] MultiplyDeBruijnBitPosition2;

        /// <summary>
        /// Find the log base 2 of an N-bit integer in O(lg(N)) operations. Value must be a power of 2.
        /// </summary>
        /// <param name="value">Must be a power of 2</param>
        /// <remarks>
        /// http://graphics.stanford.edu/~seander/bithacks.html#IntegerLogDeBruijn
        /// </remarks>
        public static uint Log2OfPowerOf2(uint value)
        {
            return MultiplyDeBruijnBitPosition2[((value * 0x077CB531U) & 0xFFFFFFFFu) >> 27];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint AlignValueCeil(uint value, uint dividor)
        {
            return (value / dividor * dividor) + dividor;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint AlignValueFloor(uint value, uint dividor)
        {
            return value / dividor * dividor;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint DivCeil(uint value, uint dividor)
        {
            return ((value - 1) / dividor) + 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong DivCeil(ulong value, ulong dividor)
        {
            return ((value - 1) / dividor) + 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static USize DivCeil(USize value, USize dividor)
        {
            return ((value - 1) / dividor) + 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint DivFloor(uint value, uint dividor)
        {
            return value / dividor;
        }

        public static void Test()
        {
            Assert.True(CeilToPowerOfTwo(125) == 128);
            Assert.True(CeilToPowerOfTwo(128) == 128);
            Assert.True(CeilToPowerOfTwo(129) == 256);

            Assert.True(FloorToPowerOfTwo(125) == 64);
            Assert.True(FloorToPowerOfTwo(128) == 128);
            Assert.True(FloorToPowerOfTwo(129) == 128);
        }

    }
}
