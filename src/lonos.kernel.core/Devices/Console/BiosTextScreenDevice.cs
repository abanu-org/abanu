using System;

namespace lonos.Kernel.Core.Devices
{

    public class BiosTextScreenDevice : IFile
    {

        public BiosTextScreenDevice()
        {
        }

        public unsafe SSize Write(byte* buf, USize count)
        {
            for (var i = 0; i < count; i++)
                Screen.Write((char)buf[i]);

            return (uint)count;
        }

    }

}
