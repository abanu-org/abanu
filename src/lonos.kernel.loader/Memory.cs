using System;
using Mosa.Kernel.x86;
using Mosa.Runtime.x86;

namespace lonos.kernel.core
{
    public class Memory
    {

        public static void Copy(Addr source, Addr destination, USize length)
        {
            for (uint i = 0; i < length; i++)
                Native.Set8(destination + i, Native.Get8(source + i));  //TODO: Optimize with Set32
        }

        public static void Copy4(Addr source, Addr destination, USize length)
        {
            var count = length / 4; //TODO: Check modulo 4 == 0
            for (uint i = 0; i < count; i += 4)
                Native.Set32(destination + i, Native.Get32(source + i));
        }

        public unsafe static void Copy4(uint source, uint destination, uint length)
        {
            var count = length / 4; //TODO: Check modulo 4 == 0
            //for (uint i = 0; i < count; i += 4)
            //{
            //    Native.Set32(destination + i, Native.Get32(source + i));
            //}

            var src = (uint*)source;
            var dst = (uint*)destination;
            for (var i = 0; i < count; i++) {
                dst[i] = src[i];
            }
        }

    }
}
