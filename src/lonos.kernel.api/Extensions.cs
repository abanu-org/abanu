using System;

namespace lonos.Kernel.Core
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
