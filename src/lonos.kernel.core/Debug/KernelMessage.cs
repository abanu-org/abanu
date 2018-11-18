using System;
using Mosa.Runtime;
using Mosa.Kernel.x86;
using Mosa.Runtime.Plug;
using Mosa.Runtime.x86;

namespace lonos.kernel.core
{
    unsafe public static class KernelMessage
    {

        public static void Setup()
        {
            Screen.Clear();
            Screen.Goto(0, 0);

            WriteLine("Setup COM1");
            Serial.SetupPort(Serial.COM1);
            WriteToSerial = true;
        }

        public static bool WriteToSerial = false;

        public static void Write(string value)
        {
            Screen.Write(value);
        }

        public static void WriteLine(string value)
        {
            Screen.Write(value);
            Screen.NextLine();
            if (WriteToSerial)
            {
                Serial.Write(Serial.COM1, value);
                Serial.Write(Serial.COM1, 10);
            }
        }

        public static void WriteLine(string format, uint arg1)
        {
            var buf = new StringBuffer();
            buf.Append(format, arg1);
            Screen.Write(buf);
            Screen.NextLine();
            if (WriteToSerial)
            {
                for (var i = 0; i < buf.Length; i++)
                {
                    Serial.Write(Serial.COM1, (byte)buf[i]);
                }
                Serial.Write(Serial.COM1, 10);
            }
        }

        public static void Write(uint value)
        {
            Screen.Write(value);
            if (WriteToSerial)
            {
                var buf = new StringBuffer(value);
                for (var i = 0; i < buf.Length; i++)
                {
                    Serial.Write(Serial.COM1, (byte)buf[i]);
                }
            }
        }

        public static void WriteLine(uint value)
        {
            Screen.Write(value);
            Screen.NextLine();
            if (WriteToSerial)
            {
                var buf = new StringBuffer(value);
                for (var i = 0; i < buf.Length; i++)
                {
                    Serial.Write(Serial.COM1, (byte)buf[i]);
                }
                Serial.Write(Serial.COM1, 10);
            }
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
            Screen.NextLine();
            if (WriteToSerial)
            {
                var buf = new StringBuffer();
                buf.Append(value);
                for (var ii = 0; ii < buf.Length; ii++)
                {
                    Serial.Write(Serial.COM1, (byte)buf[ii]);
                }
                Serial.Write(Serial.COM1, 10);
            }
        }

        public static void WriteLineHex(uint value)
        {
            Screen.Write(value, 16, -1);
            Screen.NextLine();
            if (WriteToSerial)
            {
                var buf = new StringBuffer(value, "X");
                for (var i = 0; i < buf.Length; i++)
                {
                    Serial.Write(Serial.COM1, (byte)buf[i]);
                }
                Serial.Write(Serial.COM1, 10);
            }
        }

        public static void WriteHex(uint value)
        {
            Screen.Write(value, 16, -1);
            if (WriteToSerial)
            {
                var buf = new StringBuffer(value, "X");
                for (var i = 0; i < buf.Length; i++)
                {
                    Serial.Write(Serial.COM1, (byte)buf[i]);
                }
            }
        }

    }
}
