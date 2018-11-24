using System;

namespace lonos.kernel.core
{

    public class SerialDevice : IFile
    {

        private ushort COM;

        public SerialDevice(ushort com)
        {
            COM = com;
        }

        public unsafe SSize Write(byte* buf, USize count)
        {
            for (var i = 0; i < 1; i++)
                Serial.Write(COM, buf[i]);

            return (uint)count;
        }

    }

}
