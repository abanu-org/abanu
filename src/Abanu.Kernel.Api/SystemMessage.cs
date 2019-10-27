// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Abanu.Kernel
{

    [StructLayout(LayoutKind.Explicit, Size = 4 * 7)]
    public struct SystemMessage
    {
        public const uint Size = 4 * 7;

        [FieldOffset(0)]
        public SysCallTarget Target;

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

        public SystemMessage(uint target, uint arg1 = 0, uint arg2 = 0, uint arg3 = 0, uint arg4 = 0, uint arg5 = 0, uint arg6 = 0)
        {
            Target = (SysCallTarget)target;
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
            Arg4 = arg4;
            Arg5 = arg5;
            Arg6 = arg6;
        }

        public SystemMessage(SysCallTarget target, uint arg1 = 0, uint arg2 = 0, uint arg3 = 0, uint arg4 = 0, uint arg5 = 0, uint arg6 = 0)
        {
            Target = target;
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
            Arg4 = arg4;
            Arg5 = arg5;
            Arg6 = arg6;
        }

    }

}
