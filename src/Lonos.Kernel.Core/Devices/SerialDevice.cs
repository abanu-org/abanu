// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;

namespace Lonos.Kernel.Core.Devices
{

    public class SerialDevice : IBuffer
    {

        private ushort COM;

        public SerialDevice(ushort com)
        {
            COM = com;
        }

        public unsafe SSize Read(byte* buf, USize count)
        {
            for (var i = 0; i < count; i++)
                buf[i] = Serial.Read(COM);

            return (uint)count;
        }

        public unsafe SSize Write(byte* buf, USize count)
        {
            for (var i = 0; i < count; i++)
                Serial.Write(COM, buf[i]);

            return (uint)count;
        }

    }

}
