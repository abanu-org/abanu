using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;

namespace lonos.kernel.core.Diagnostics
{
    public static class AsmDebugFunction
    {
        [DllImport("lonos.DebugFunction1.o", EntryPoint = "DebugFunction1")]
        public extern static void DebugFunction1();


    }
}
