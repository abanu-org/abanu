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
            SysCallTest(new SysCallArgs { Arg1 = 0x55 });
            while (true) { }
        }

        [DllImport("x86/app.HelloKernel.o", EntryPoint = "SysCallInt")]
        public extern static uint SysCallInt(SysCallArgs args);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static uint SysCall(SysCallArgs args)
        {
            var result = SysCallInt(args);
            return result;
        }

        public static Addr RequestPages(USize pages)
        {
            return SysCall(new SysCallArgs { Command = 22, Arg1 = 0x31, Arg2 = 0x32, Arg3 = 0x33, Arg4 = 0x34, Arg5 = 0x35, Arg6 = 0x36 });
        }

        public static Addr ServiceFunc1(SysCallArgs args)
        {
            return SysCall(args);
        }

        public static void SysCallTest(SysCallArgs args)
        {
            //var addr = RequestPages(38);
            var result = ServiceFunc1(new SysCallArgs { Command = 22, Arg1 = 0x31, Arg2 = 0x32, Arg3 = 0x33, Arg4 = 0x34, Arg5 = 0x35, Arg6 = 0x36 });
            result++;
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
