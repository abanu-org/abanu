using System;

namespace lonos.kernel.core
{

    public class ConsoleDevice : IFile
    {

        private IFile Device;

        public ConsoleDevice(IFile device)
        {
            Device = device;
        }

        public void SetOutputDevice(IFile device)
        {
            Device = device;
        }

        public unsafe SSize Write(byte* buf, USize count)
        {
            return Device.Write(buf, count);
        }

    }

}
