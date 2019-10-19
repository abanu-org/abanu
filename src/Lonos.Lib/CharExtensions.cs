// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lonos
{
    public static class CharExtensions
    {

        private const byte NullDigitByte = (byte)'0';

        public static byte GetNumber(this char digit)
        {
            return (byte)((byte)digit - NullDigitByte);
        }

        public static uint ParseUInt32(this IList<char> s)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));

            if (s.Count == 0)
                throw new FormatException();

            uint result;
            if (TryParseUInt32(s, out result))
                return result;

            throw new FormatException();
        }

        public static uint ParseUInt32(this IList<char> s, int start, int count)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));

            if (s.Count == 0)
                throw new FormatException();

            uint result;
            if (TryParseUInt32(s, start, count, out result))
                return result;

            throw new FormatException();
        }

        public static bool TryParseUInt32(this IList<char> s, out uint result)
        {
            return TryParseUInt32(s, 0, s.Count, out result);
        }

        public static bool TryParseUInt32(this IList<char> s, int start, int count, out uint result)
        {
            int end = start + count;
            uint n = 0;
            result = 0;
            var i = start;
            while (i < end)
            {
                if (n > (0xFFFFFFFF / 10))
                {
                    return false;
                }
                n *= 10;
                if (s[i] != '\0')
                {
                    uint newN = n + (uint)(s[i] - '0');
                    // Detect an overflow here...
                    if (newN < n)
                    {
                        return false;
                    }
                    n = newN;
                }
                i++;
            }
            result = n;
            return true;
        }

    }
}
