using System;
using Mosa.Runtime;
using Mosa.Kernel.x86;
using Mosa.Runtime.Plug;
using Mosa.Runtime.x86;

namespace lonos.kernel.core
{
    unsafe public static class Debug
    {

        public static void Setup()
        {
            Screen.Clear();
            Screen.Goto(0, 0);
            currLine = 0;
        }

        private static uint currLine = 0;

        public static void Write(string value)
        {
            Screen.Write(value);
        }


        public static void WriteLine(string value)
        {
            Screen.Write(value);
            Screen.Goto(++currLine, 0);
        }

        public static void Write(uint value)
        {
            Screen.Write(value);
        }

        public static void WriteLine(uint value)
        {
            Screen.Write(value);
            Screen.Goto(++currLine, 0);
        }

        public static void WriteLine(NullTerminatedString* value)
        {
            var i = 0;
            while (true)
            {
                var b = value->Bytes[i];
                if (b == 0)
                    break;
                Screen.Write((char)b);
                i++;
            }
            Screen.Goto(++currLine, 0);
        }

        public static void WriteLineHex(uint value)
        {
            Screen.Write(value, 16, -1);
            Screen.Goto(++currLine, 0);
        }

        public static void WriteHex(uint value)
        {
            Screen.Write(value, 16, -1);
        }

        public static void Break()
        {
            Debug.Write("BREAK");
            while (true)
            {
                Native.Nop();
            }
        }

    }
}
