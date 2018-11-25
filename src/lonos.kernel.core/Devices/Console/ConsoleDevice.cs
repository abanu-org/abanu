using System;

namespace lonos.kernel.core
{

    public class ConsoleDevice : IFile
    {
 
        public ConsoleDevice()
        {
        }

        public unsafe SSize Write(byte* buf, USize count)
        {
            return Devices.Screen.Write(buf, count);
        }

    }

}
