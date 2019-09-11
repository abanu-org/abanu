// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;

namespace Lonos.Kernel.Core
{
    public static class NumberExtensions
    {
        public static int ToInt(this bool self)
        {
            return self ? 1 : 0;
        }

        public static byte ToByte(this bool self)
        {
            return self ? (byte)1 : (byte)0;
        }

        public static char ToChar(this bool self)
        {
            return self ? '1' : '0';
        }

        public static int ToInt(this Enum self)
        {
            return (int)(object)self;
        }

        public static string ToStringNumber(this Enum self)
        {
            return ((int)(object)self).ToString();
        }

        public static string ToHex(this uint self)
        {
            return self.ToString("X");
        }

        public static string ToHex(this byte self)
        {
            return self.ToString("X");
        }
    }
}
