// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using Mosa.Runtime;
using Mosa.Runtime.Plug;
using Mosa.Runtime.x86;

namespace Lonos.Kernel.Core
{
    public static unsafe class Debug
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
