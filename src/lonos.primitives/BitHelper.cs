namespace lonos
{
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
}
