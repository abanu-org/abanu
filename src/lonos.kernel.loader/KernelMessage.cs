using System;
namespace lonos.kernel.core
{

    public class KernelMessageWriter : IBufferWriter
    {

        public KernelMessageWriter()
        {
            Serial.SetupPort(Serial.COM1);
        }

        public unsafe SSize Write(byte* buf, USize count)
        {
            for (var i = 0; i < count; i++)
                Serial.Write(Serial.COM1, buf[i]);
            return (uint)count;
        }
    }

    public static class KernelMessage_
    {

        public static void WriteLine(string msg)
        {
            Serial.Write(Serial.COM1, msg);
        }

        public static void WriteLine(StringBuffer msg)
        {
            for (var i = 0; i < msg.Length; i++)
                Serial.Write(Serial.COM1, (byte)msg[i]);
            Serial.Write(Serial.COM1, (byte)'\n');
        }

        public static void WriteLine(uint num)
        {
            var sb = new StringBuffer();
            sb.Append(num, 16);
            WriteLine(sb);
        }


    }

}
