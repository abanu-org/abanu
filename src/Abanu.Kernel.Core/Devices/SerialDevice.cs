// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;

namespace Abanu.Kernel.Core.Devices
{

    /// <summary>
    /// Wrapper for the Serial interface
    /// </summary>
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
