// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using Lonos.Kernel.Core.Devices;

namespace Lonos.Kernel.Core.Diagnostics
{

    public class KernelMessageDevice : IFile
    {

        public KernelMessageDevice()
        {
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
