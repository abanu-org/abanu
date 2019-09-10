using lonos.Kernel.Core.Devices;
using System;

namespace lonos.Kernel.Core.Diagnostics
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
