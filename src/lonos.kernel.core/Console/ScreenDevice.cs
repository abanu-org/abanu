using System;

namespace lonos.kernel.core
{

    public class ScreenDevice : IFile
    {
 
        public ScreenDevice()
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
