using System;
using Mosa.Runtime;
using Mosa.Kernel.x86;
using Mosa.Runtime.Plug;
using Mosa.Runtime.x86;

namespace lonos.kernel.core
{
    unsafe public static class Debug
    {

        public static void Setup()
        {
        }

        public static void Break()
        {
            while (true)
            {
                Native.Nop();
            }
        }

    }
}
