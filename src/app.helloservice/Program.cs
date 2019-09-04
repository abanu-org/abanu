using lonos.Kernel.Core;
using Mosa.Runtime.x86;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace lonos.Kernel
{
    public static class Program
    {
        // TODO: Rename to Init
        public static void Main()
        {
        }

        public static void Func1()
        {
            SysCall(21, 69);
        }

        // TODO: Naming!
        [DllImport("x86/app.HelloKernel.o", EntryPoint = "SysCallInt")]
        public extern static uint SysCallInt(uint command, uint arg1);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static uint SysCall(uint command, uint arg1)
        {
            var result = SysCallInt(command, arg1);
            return result;
        }

        private static void Dummy()
        {
            //This is a dummy call, that get never executed.
            //Its requied, because we need a real reference to Mosa.Runtime.x86
            //Without that, the .NET compiler will optimize that reference away
            //if its nowhere used. Than the Compiler dosnt know about that Refernce
            //and the Compilation will fail
            Mosa.Runtime.x86.Internal.GetStackFrame(0);
        }
    }
}
