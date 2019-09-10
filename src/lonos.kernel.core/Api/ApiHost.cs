// Copyright (c) Lonos Project. All rights reserved.
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

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
