using System;

namespace lonos.kernel.core
{

    public class KernelMessageDevice : IFile
    {

        public KernelMessageDevice()
        {
        }

        public unsafe SSize Write(byte* buf, USize count)
        {
            var devSerial = Devices.COM1;
            var devScreen = Devices.Screen;

            if (devSerial == null && devScreen == null)
                return 0;

            if (devSerial == null)
                return devScreen.Write(buf, count);

            if (devScreen == null)
                return devSerial.Write(buf, count);

            var writtenSerial = devSerial.Write(buf, count);
            var writtenScreen = devScreen.Write(buf, count);
            if (writtenSerial < writtenScreen)
                return writtenSerial;
            else
                return writtenScreen;
        }

    }

}
