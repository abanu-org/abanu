// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

namespace Lonos
{
    public static class BitConversion
    {

        public static unsafe byte[] GetBytes(uint value)
        {
            byte[] bytes = new byte[sizeof(uint)];
            fixed (byte* ptr = &bytes[0])
                ((uint*)ptr)[0] = value;
            return bytes;
        }

        public static unsafe int GetInt32(byte[] bytes)
        {
            fixed (byte* ptr = &bytes[0])
                return ((int*)ptr)[0];
        }

    }
}
