// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;

namespace Lonos.Kernel.Core.Api
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
