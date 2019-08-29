using System;
namespace lonos.Kernel.Core.Api
{

    internal class ApiHost : IKernelApi
    {

        public int FileWrite(IntPtr ptr, USize elementSize, USize elements, FileHandle stream)
        {
            throw new NotImplementedException();
        }

        public int FileWrite(NullTerminatedString str, FileHandle stream)
        {
            throw new NotImplementedException();
        }

        public SSize FileWrite(FileHandle file, IntPtr buf, USize count)
        {
            throw new NotImplementedException();
        }

    }

}
