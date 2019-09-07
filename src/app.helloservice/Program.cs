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
    [StructLayout(LayoutKind.Explicit, Size = 4 * 7)]
    public struct SysCallArgs
    {
        public const uint Size = 4 * 7;

        [FieldOffset(0)]
        public uint Command;

        [FieldOffset(4)]
        public uint Arg1;

        [FieldOffset(8)]
        public uint Arg2;

        [FieldOffset(12)]
        public uint Arg3;

        [FieldOffset(16)]
        public uint Arg4;

        [FieldOffset(20)]
        public uint Arg5;

        [FieldOffset(24)]
        public uint Arg6;
    }

    //[StructLayout(LayoutKind.Sequential, Size = 4 * 5)]
    //public struct ServiceArgs
    //{
    //    public const uint Size = 4 * 5;

    //    public uint Arg1;
    //    public uint Arg2;
    //    public uint Arg3;
    //    public uint Arg4;
    //    public uint Arg5;
    //}

    public static class Program
    {

        public static void Main()
        {
            while (true) { }
        }

        public static void Func1(SysCallArgs args)
        {
            var arg2 = args.Arg1 += 10;
            //uint arg2 = 60;
            SysCall(new SysCallArgs { Command = 21, Arg1 = arg2 }); // 21=Service Return, 69=result
        }

        // TODO: Naming!
        [DllImport("x86/app.HelloKernel.o", EntryPoint = "SysCallInt")]
        public extern static uint SysCallInt(SysCallArgs args);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static uint SysCall(SysCallArgs args)
        {
            var result = SysCallInt(args);
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
