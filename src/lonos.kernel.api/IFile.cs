using System;

namespace lonos.kernel.core
{

    public interface IFile
    {
        unsafe SSize Write(byte* buf, USize count);
    }

}
