using System;

namespace lonos.Kernel.Core.Devices
{

    public class NullDevice : IFile
    {

        public NullDevice()
        {
        }

        public unsafe SSize Write(byte* buf, USize count)
        {
            return (uint)count;
        }

    }

}
