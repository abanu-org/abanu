// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Abanu.Kernel.Core.Diagnostics
{

    // Currently unused
    internal static class AsmDebugFunction
    {
        [DllImport("x86/Abanu.DebugFunction1.o", EntryPoint = "DebugFunction1")]
        public static extern void DebugFunction1();

    }
}
