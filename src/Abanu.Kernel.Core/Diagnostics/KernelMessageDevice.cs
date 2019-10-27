// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using Abanu.Kernel.Core.Devices;

namespace Abanu.Kernel.Core.Diagnostics
{

    public class KernelMessageDevice : IBuffer
    {

        public KernelMessageDevice()
        {
        }

        public unsafe SSize Read(byte* buf, USize count)
        {
            return 0;
        }

        public unsafe SSize Write(byte* buf, USize count)
        {
            var devSerial = DeviceManager.Serial1;
            var devConsole = DeviceManager.Console;

            if (devSerial == null && devConsole == null)
                return 0;

            if (devSerial == null)
                return devConsole.Write(buf, count);

            if (devConsole == null)
                return devSerial.Write(buf, count);

            var writtenSerial = devSerial.Write(buf, count);
            var writtenConsole = devConsole.Write(buf, count);
            if (writtenSerial < writtenConsole)
                return writtenSerial;
            else
                return writtenConsole;
        }

    }

}
