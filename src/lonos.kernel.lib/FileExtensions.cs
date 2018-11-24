using System;

namespace lonos.kernel.core
{
    public static class FileExtensions
    {

        public static void Write(this IFile file, string value)
        {
            for (var i = 0; i < value.Length; i++)
            {
                file.Write(value[i]);
            }
        }

        public unsafe static void Write(this IFile file, char value)
        {
            var b = (byte)value;
            byte* ptr = &b;
            file.Write(ptr, 1);
        }

        public unsafe static void Write(this IFile file, byte value)
        {
            byte* ptr = &value;
            file.Write(ptr, 1);
        }

    }
}