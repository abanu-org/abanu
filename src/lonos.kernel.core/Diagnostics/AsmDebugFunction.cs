using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace lonos.Kernel.Core.Diagnostics
{
    public static class AsmDebugFunction
    {
        [DllImport("x86/lonos.DebugFunction1.o", EntryPoint = "DebugFunction1")]
        public static extern void DebugFunction1();

    }
}
