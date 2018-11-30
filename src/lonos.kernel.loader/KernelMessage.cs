using System;
namespace lonos.kernel.core
{

    public static class KernelMessage
    {

        public static void WriteLine(string msg)
        {
            Serial.Write(Serial.COM1, msg);
        }

        public static void WriteLine(StringBuffer msg)
        {
            for (var i = 0; i < msg.Length; i++)
                Serial.Write(Serial.COM1, (byte)msg[i]);
        }

        public static void WriteLine(uint num) {
            var sb = new StringBuffer();
            sb.Append(num, 16);
            WriteLine(sb);
        }


    }

}
