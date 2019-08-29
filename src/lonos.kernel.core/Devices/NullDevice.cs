using System;

namespace lonos.kernel.core.Devices
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
