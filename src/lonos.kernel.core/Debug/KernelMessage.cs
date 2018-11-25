using System;
using Mosa.Runtime;
using Mosa.Kernel.x86;
using Mosa.Runtime.Plug;
using Mosa.Runtime.x86;

namespace lonos.kernel.core
{
    unsafe public static class KernelMessage
    {

        private static IFile Dev
        {
            get
            {
                return Devices.KMsg;
            }
        }

        public static void Write(string value)
        {
            Dev.Write(value);
        }

        public static void Write(char value)
        {
            Dev.Write(value);
        }

        public static void WriteLine(string value)
        {
            Dev.Write(value);
            Dev.Write('\n');
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

        public static void WriteLine(string format, uint arg1)
        {
            var buf = new StringBuffer();
            buf.Append(format, arg1);
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
