// Copyright (c) Lonos Project. All rights reserved.
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
