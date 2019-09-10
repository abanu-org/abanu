using System;
using Mosa.Runtime;
using Mosa.Runtime.Plug;
using Mosa.Runtime.x86;

namespace Lonos.Kernel.Core
{
    unsafe public static class Debug
    {

        public static void Setup()
        {
        }

        public static void Break()
        {
            KernelMessage.Write("<BREAK>");
            while (true)
            {
                Native.Nop();
            }
        }

    }
}
