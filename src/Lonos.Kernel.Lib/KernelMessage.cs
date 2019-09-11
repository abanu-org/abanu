﻿// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using Mosa.Runtime;
using Mosa.Runtime.Plug;
using Mosa.Runtime.x86;

namespace Lonos.Kernel.Core
{

    public static unsafe class KernelMessage
    {

        private static IBufferWriter Dev;

        public static void SetHandler(IBufferWriter handler)
        {
            Dev = handler;
        }

        public static void Write(string value)
        {
            Dev.Write(value);
        }

        public static void Write(char value)
        {
            Dev.Write(value);
        }

        public static void WriteLine()
        {
            Dev.Write('\n');
        }

        public static void WriteLine(string value)
        {
            Dev.Write(value);
            Dev.Write('\n');
        }

        public static void WriteLine(string format, bool arg0)
        {
            var buf = new StringBuffer();
            buf.Append(format, arg0 == true ? "yes" : "no");
            buf.Append('\n');
            buf.WriteTo(Dev);
        }

        public static void WriteLine(string format, string arg0)
        {
            var buf = new StringBuffer();
            buf.Append(format, arg0);
            buf.Append('\n');
            buf.WriteTo(Dev);
        }

        public static void WriteLine(string format, string arg0, uint arg1)
        {
            var buf = new StringBuffer();
            buf.Append(format, arg0, arg1); //TODO
            buf.Append('\n');
            buf.WriteTo(Dev);
        }

        public static void Path(string prefix, string value)
        {
            Write(prefix);
            Write(": ");
            WriteLine(value);
        }

        public static void Path(string prefix, string format, uint arg0)
        {
            Write(prefix);
            Write(": ");
            WriteLine(format, arg0);
        }

        public static void Path(string prefix, string format, uint arg0, uint arg1)
        {
            Write(prefix);
            Write(": ");
            WriteLine(format, arg0, arg1);
        }

        public static void Path(string prefix, string format, uint arg0, uint arg1, uint arg2)
        {
            Write(prefix);
            Write(": ");
            WriteLine(format, arg0, arg1, arg2);
        }

        public static void Write(string format, uint arg0)
        {
            var buf = new StringBuffer();
            buf.Append(format, arg0);
            buf.WriteTo(Dev);
        }

        public static void Write(string format, uint arg0, uint arg1)
        {
            var buf = new StringBuffer();
            buf.Append(format, arg0, arg1);
            buf.WriteTo(Dev);
        }

        public static void Write(string format, uint arg0, uint arg1, uint arg2)
        {
            var buf = new StringBuffer();
            buf.Append(format, arg0, arg1, arg2);
            buf.WriteTo(Dev);
        }

        public static void Write(string format, uint arg0, uint arg1, uint arg2, uint arg3)
        {
            var buf = new StringBuffer();
            buf.Append(format, arg0, arg1, arg2, arg3);
            buf.WriteTo(Dev);
        }

        public static void Write(string format, uint arg0, uint arg1, uint arg2, uint arg3, uint arg4)
        {
            var buf = new StringBuffer();
            buf.Append(format, arg0, arg1, arg2, arg3, arg4);
            buf.WriteTo(Dev);
        }

        public static void WriteLine(string format, uint arg0)
        {
            var buf = new StringBuffer();
            buf.Append(format, arg0);
            buf.Append('\n');
            buf.WriteTo(Dev);
        }

        public static void WriteLine(string format, int arg0)
        {
            var buf = new StringBuffer();
            buf.Append(format, (uint)arg0); //TODO
            buf.Append('\n');
            buf.WriteTo(Dev);
        }

        public static void WriteLine(string format, int arg0, int arg1)
        {
            var buf = new StringBuffer();
            buf.Append(format, (uint)arg0, (uint)arg1); //TODO
            buf.Append('\n');
            buf.WriteTo(Dev);
        }

        public static void WriteLine(string format, int arg0, int arg1, int arg2)
        {
            var buf = new StringBuffer();
            buf.Append(format, (uint)arg0, (uint)arg1, (uint)arg2); //TODO
            buf.Append('\n');
            buf.WriteTo(Dev);
        }

        public static void WriteLine(string format, uint arg0, uint arg1)
        {
            var buf = new StringBuffer();
            buf.Append(format, arg0, arg1);
            buf.Append('\n');
            buf.WriteTo(Dev);
        }

        public static void WriteLine(string format, uint arg0, uint arg1, uint arg2)
        {
            var buf = new StringBuffer();
            buf.Append(format, arg0, arg1, arg2);
            buf.Append('\n');
            buf.WriteTo(Dev);
        }

        public static void WriteLine(string format, uint arg0, uint arg1, uint arg2, uint arg3)
        {
            var buf = new StringBuffer();
            buf.Append(format, arg0, arg1, arg2, arg3);
            buf.Append('\n');
            buf.WriteTo(Dev);
        }

        public static void WriteLine(string format, uint arg0, uint arg1, uint arg2, uint arg3, uint arg4)
        {
            var buf = new StringBuffer();
            buf.Append(format, arg0, arg1, arg2, arg3, arg4);
            buf.Append('\n');
            buf.WriteTo(Dev);
        }

        public static void WriteLine(string format, uint arg0, uint arg1, uint arg2, uint arg3, uint arg4, uint arg5)
        {
            var buf = new StringBuffer();
            buf.Append(format, arg0, arg1, arg2, arg3, arg4, arg5);
            buf.Append('\n');
            buf.WriteTo(Dev);
        }

        public static void WriteLine(string format, uint arg0, uint arg1, uint arg2, uint arg3, uint arg4, uint arg5, uint arg6)
        {
            var buf = new StringBuffer();
            buf.Append(format, arg0, arg1, arg2, arg3, arg4, arg5, arg6);
            buf.Append('\n');
            buf.WriteTo(Dev);
        }

        public static void Write(uint value)
        {
            var sb = new StringBuffer();
            sb.Append(value);
            sb.WriteTo(Dev);
        }

        public static void WriteLine(uint value)
        {
            var sb = new StringBuffer();
            sb.Append(value);
            sb.Append('\n');
            sb.WriteTo(Dev);
        }

        public static void WriteLine(NullTerminatedString* value)
        {
            var sb = new StringBuffer();
            sb.Append(value);
            sb.Append('\n');
            sb.WriteTo(Dev);
        }

        public static void WriteLineHex(uint value)
        {
            var buf = new StringBuffer(value, "X");
            buf.Append('\n');
            buf.WriteTo(Dev);
        }

        public static void WriteHex(uint value)
        {
            var buf = new StringBuffer(value, "X");
            buf.WriteTo(Dev);
        }

        public static void Write(StringBuffer sb)
        {
            for (var i = 0; i < sb.Length; i++)
            {
                Write(sb[i]);
            }
        }

        public static void WriteLine(StringBuffer sb)
        {
            Write(sb);
            Write('\n');
        }

    }
}