using System;

namespace Lonos.Kernel.Core
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

        public unsafe static void Write(this IBufferWriter file, char value)
        {
            var b = (byte)value;
            byte* ptr = &b;
            file.Write(ptr, 1);
        }

        public unsafe static void Write(this IBufferWriter file, byte value)
        {
            byte* ptr = &value;
            file.Write(ptr, 1);
        }

    }
}