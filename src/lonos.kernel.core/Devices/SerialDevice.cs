// Copyright (c) Lonos Project. All rights reserved.
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;

namespace lonos.Kernel.Core.Devices
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
