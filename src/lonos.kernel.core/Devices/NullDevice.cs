// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

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
