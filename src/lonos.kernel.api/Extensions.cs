using System;

namespace lonos.kernel.core
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

    public static class BitHelper
    {
        #region Byte

        public static bool IsMaskSet(this byte self, byte mask)
        {
            return (self & mask) == mask;
        }

        public static bool IsBitSet(this byte self, byte bit)
        {
            return (self & (0x1 << bit)) == (0x1 << bit);
        }

        public static byte SetMask(this byte self, byte mask)
        {
            return (byte)(self | mask);
        }

        public static byte SetBit(this byte self, byte bit)
        {
            return (byte)(self | (0x1 << bit));
        }

        public static byte ClearMask(this byte self, byte mask)
        {
            return (byte)(self & ~mask);
        }

        public static byte ClearBit(this byte self, byte bit)
        {
            return (byte)(self & ~(0x1 << bit));
        }

        public static byte SetMask(this byte self, byte mask, bool state)
        {
            if (state)
                return (byte)(self | mask);
            else
                return (byte)(self & ~mask);
        }

        public static byte SetBit(this byte self, byte bit, bool state)
        {
            if (state)
                return (byte)(self | (0x1 << bit));
            else
                return (byte)(self & ~(0x1 << bit));
        }

        public static byte CircularLeftShift(this byte a, byte n)
        {
            return (byte)(a << n | a >> (8 - n));
        }

        public static byte CircularRightShift(this byte a, byte n)
        {
            return (byte)(a >> n | a << (8 - n));
        }

        public static byte GetBits(this byte self, byte index, byte count)
        {
            return (byte)((self >> index) << (8 - count));
        }

        public static byte SetBits(this byte self, byte index, byte count, byte value)
        {
            byte mask = (byte)(0xFF >> (8 - count));
            byte bits = (byte)((value & mask) << index);
            return (byte)((self & ~(mask << index)) | bits);
        }

        #endregion Byte

        #region uint

        public static bool IsMaskSet(this uint self, uint mask)
        {
            return (self & mask) == mask;
        }

        public static bool IsBitSet(this uint self, byte bit)
        {
            return (self & (0x1 << bit)) == (0x1 << bit);
        }

        public static uint SetMask(this uint self, byte mask)
        {
            return self | mask;
        }

        public static uint SetBit(this uint self, byte bit)
        {
            return self | (0x1U << bit);
        }

        public static uint ClearMask(this uint self, uint mask)
        {
            return self & ~mask;
        }

        public static uint ClearBit(this uint self, byte bit)
        {
            return self & ~(0x1U << bit);
        }

        public static uint SetMask(this uint self, uint mask, bool state)
        {
            if (state)
                return self | mask;
            else
                return self & ~mask;
        }

        public static uint SetBit(this uint self, byte bit, bool state)
        {
            if (state)
                return self | (0x1U << bit);
            else
                return self & ~(0x1U << bit);
        }

        public static uint CircularLeftShift(this uint a, byte n)
        {
            return a << n | a >> (32 - n);
        }

        public static uint CircularRightShift(this uint a, byte n)
        {
            return a >> n | a << (32 - n);
        }

        public static uint GetBits(this uint self, byte index, byte count)
        {
            return (self >> index) << (32 - count);
        }

        public static uint GetBits(this uint self, byte index, byte count, byte sourceIndex)
        {
            return ((self >> index) << (32 - count)) << sourceIndex;
        }

        public static uint SetBits(this uint self, byte index, byte count, uint value)
        {
            uint mask = 0xFFFFFFFFU >> (32 - count);
            uint bits = (value & mask) << index;
            return (self & ~(mask << index)) | bits;
        }

        public static uint SetBits(this uint self, byte index, byte count, uint value, byte sourceIndex)
        {
            value = value >> sourceIndex;
            uint mask = 0xFFFFFFFFU >> (32 - count);
            uint bits = (value & mask) << index;
            return (self & ~(mask << index)) | bits;
        }

        #endregion uint

        #region ulong

        public static bool IsMaskSet(this ulong self, ulong mask)
        {
            return (self & mask) == mask;
        }

        public static bool IsBitSet(this ulong self, byte bit)
        {
            return (self & (0x1u << bit)) == (0x1u << bit);
        }

        public static ulong SetMask(this ulong self, byte mask)
        {
            return self | mask;
        }

        public static ulong SetBit(this ulong self, byte bit)
        {
            return self | (0x1U << bit);
        }

        public static ulong ClearMask(this ulong self, ulong mask)
        {
            return self & ~mask;
        }

        public static ulong ClearBit(this ulong self, byte bit)
        {
            return self & ~(0x1U << bit);
        }

        public static ulong SetMask(this ulong self, uint mask, bool state)
        {
            if (state)
                return self | mask;
            else
                return self & ~mask;
        }

        public static ulong SetBit(this ulong self, byte bit, bool state)
        {
            if (state)
                return self | (0x1U << bit);
            else
                return self & ~(0x1U << bit);
        }

        public static ulong CircularLeftShift(this ulong a, byte n)
        {
            return a << n | a >> (64 - n);
        }

        public static ulong CircularRightShift(this ulong a, byte n)
        {
            return a >> n | a << (64 - n);
        }

        public static ulong GetBits(this ulong self, byte index, byte count)
        {
            return (self >> index) << (64 - count);
        }

        public static ulong GetBits(this ulong self, byte index, byte count, byte sourceIndex)
        {
            return ((self >> index) << (64 - count)) << sourceIndex;
        }

        public static ulong SetBits(this ulong self, byte index, byte count, ulong value)
        {
            ulong mask = 0xFFFFFFFFFFFFFFFFU >> (64 - count);
            ulong bits = (value & mask) << index;
            return (self & ~(mask << index)) | bits;
        }

        public static ulong SetBits(this ulong self, byte index, byte count, ulong value, byte sourceIndex)
        {
            value = value >> sourceIndex;
            ulong mask = 0xFFFFFFFFFFFFFFFFU >> (64 - count);
            ulong bits = (value & mask) << index;
            return (self & ~(mask << index)) | bits;
        }

        #endregion uint

        #region *Byte

        unsafe public static bool IsMaskSet(byte* self, byte mask)
        {
            return (*self & mask) == mask;
        }

        unsafe public static bool IsBitSet(byte* self, byte bit)
        {
            return (*self & (0x1 << bit)) == (0x1 << bit);
        }

        unsafe public static void SetMask(byte* self, byte mask)
        {
            *self = (byte)(*self | mask);
        }

        unsafe public static void SetBit(byte* self, byte bit)
        {
            *self = (byte)(*self | (0x1 << bit));
        }

        unsafe public static void ClearMask(byte* self, byte mask)
        {
            *self = (byte)(*self & ~mask);
        }

        unsafe public static void ClearBit(byte* self, byte bit)
        {
            *self = (byte)(*self & ~(0x1 << bit));
        }

        unsafe public static void SetMask(byte* self, byte mask, bool state)
        {
            if (state)
                *self = (byte)(*self | mask);
            else
                *self = (byte)(*self & ~mask);
        }

        unsafe public static void SetBit(byte* self, byte bit, bool state)
        {
            if (state)
                *self = (byte)(*self | (0x1 << bit));
            else
                *self = (byte)(*self & ~(0x1 << bit));
        }

        unsafe public static void CircularLeftShift(byte* a, byte n)
        {
            *a = (byte)(*a << n | *a >> (8 - n));
        }

        unsafe public static void CircularRightShift(byte* a, byte n)
        {
            *a = (byte)(*a >> n | *a << (8 - n));
        }

        unsafe public static void GetBits(byte* self, byte index, byte count)
        {
            *self = (byte)((*self >> index) << (8 - count));
        }

        unsafe public static void SetBits(byte* self, byte source, byte index, byte count)
        {
            byte mask = (byte)(0xFF >> (8 - count));
            byte bits = (byte)((source & mask) << index);
            *self = (byte)((*self & ~(mask << index)) | bits);
        }

        #endregion *Byte
    }

    /// <summary>
    /// Represents the HEX value of a bit position
    /// </summary>
    public static class BitMask
    {
        public const uint Bit0 = 1u << 0;
        public const uint Bit1 = 1u << 1;
        public const uint Bit2 = 1u << 2;
        public const uint Bit3 = 1u << 3;
        public const uint Bit4 = 1u << 4;
        public const uint Bit5 = 1u << 5;
        public const uint Bit6 = 1u << 6;
        public const uint Bit7 = 1u << 7;
        public const uint Bit8 = 1u << 8;
        public const uint Bit9 = 1u << 9;
        public const uint Bit10 = 1u << 10;
        public const uint Bit11 = 1u << 11;
        public const uint Bit12 = 1u << 12;
        public const uint Bit13 = 1u << 13;
        public const uint Bit14 = 1u << 14;
        public const uint Bit15 = 1u << 15;
        public const uint Bit16 = 1u << 16;
        public const uint Bit17 = 1u << 17;
        public const uint Bit18 = 1u << 18;
        public const uint Bit19 = 1u << 19;
        public const uint Bit20 = 1u << 20;
        public const uint Bit21 = 1u << 21;
        public const uint Bit22 = 1u << 22;
        public const uint Bit23 = 1u << 23;
        public const uint Bit24 = 1u << 24;
        public const uint Bit25 = 1u << 25;
        public const uint Bit26 = 1u << 26;
        public const uint Bit27 = 1u << 27;
        public const uint Bit28 = 1u << 28;
        public const uint Bit29 = 1u << 29;
        public const uint Bit30 = 1u << 30;
        public const uint Bit31 = 1u << 31;
    }

    public static class BitMask64
    {
        public const ulong Bit0 = 1u << 0;
        public const ulong Bit1 = 1u << 1;
        public const ulong Bit2 = 1u << 2;
        public const ulong Bit3 = 1u << 3;
        public const ulong Bit4 = 1u << 4;
        public const ulong Bit5 = 1u << 5;
        public const ulong Bit6 = 1u << 6;
        public const ulong Bit7 = 1u << 7;
        public const ulong Bit8 = 1u << 8;
        public const ulong Bit9 = 1u << 9;
        public const ulong Bit10 = 1u << 10;
        public const ulong Bit11 = 1u << 11;
        public const ulong Bit12 = 1u << 12;
        public const ulong Bit13 = 1u << 13;
        public const ulong Bit14 = 1u << 14;
        public const ulong Bit15 = 1u << 15;
        public const ulong Bit16 = 1u << 16;
        public const ulong Bit17 = 1u << 17;
        public const ulong Bit18 = 1u << 18;
        public const ulong Bit19 = 1u << 19;
        public const ulong Bit20 = 1u << 20;
        public const ulong Bit21 = 1u << 21;
        public const ulong Bit22 = 1u << 22;
        public const ulong Bit23 = 1u << 23;
        public const ulong Bit24 = 1u << 24;
        public const ulong Bit25 = 1u << 25;
        public const ulong Bit26 = 1u << 26;
        public const ulong Bit27 = 1u << 27;
        public const ulong Bit28 = 1u << 28;
        public const ulong Bit29 = 1u << 29;
        public const ulong Bit30 = 1u << 30;
        public const ulong Bit31 = 1u << 31;

        public const ulong Bit32 = 1u << 32;
        public const ulong Bit33 = 1u << 33;
        public const ulong Bit34 = 1u << 34;
        public const ulong Bit35 = 1u << 35;
        public const ulong Bit36 = 1u << 36;
        public const ulong Bit37 = 1u << 37;
        public const ulong Bit38 = 1u << 38;
        public const ulong Bit39 = 1u << 39;
        public const ulong Bit40 = 1u << 40;
        public const ulong Bit41 = 1u << 41;
        public const ulong Bit42 = 1u << 42;
        public const ulong Bit43 = 1u << 43;
        public const ulong Bit44 = 1u << 44;
        public const ulong Bit45 = 1u << 45;
        public const ulong Bit46 = 1u << 46;
        public const ulong Bit47 = 1u << 47;
        public const ulong Bit48 = 1u << 48;
        public const ulong Bit49 = 1u << 49;
        public const ulong Bit50 = 1u << 50;
        public const ulong Bit51 = 1u << 51;
        public const ulong Bit52 = 1u << 52;
        public const ulong Bit53 = 1u << 53;
        public const ulong Bit54 = 1u << 54;
        public const ulong Bit55 = 1u << 55;
        public const ulong Bit56 = 1u << 56;
        public const ulong Bit57 = 1u << 57;
        public const ulong Bit58 = 1u << 58;
        public const ulong Bit59 = 1u << 59;
        public const ulong Bit60 = 1u << 60;
        public const ulong Bit61 = 1u << 61;
        public const ulong Bit62 = 1u << 62;
        public const ulong Bit63 = 1u << 63;
    }
}
