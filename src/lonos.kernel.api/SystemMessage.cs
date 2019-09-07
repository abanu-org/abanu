using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace lonos.Kernel
{

    [StructLayout(LayoutKind.Explicit, Size = 4 * 7)]
    public struct SystemMessage
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

}
