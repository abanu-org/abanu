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
        public static void Main()
        {
            SysCallTest();
            while (true) { }
        }

        [DllImport("x86/app.HelloKernel.o", EntryPoint = "SysCallInt")]
        public extern static uint SysCallInt(uint command, uint arg1);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static uint SysCall(uint command, uint arg1)
        {
            var result = SysCallInt(command, arg1);
            return result;
        }

        public static Addr RequestPages(USize pages)
        {
            return SysCall(20, pages);
        }

        public static Addr ServiceFunc1(uint demoArg)
        {
            return SysCall(22, demoArg);
        }

        public static void SysCallTest()
        {
            //var addr = RequestPages(38);
            var result = ServiceFunc1(38);
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
