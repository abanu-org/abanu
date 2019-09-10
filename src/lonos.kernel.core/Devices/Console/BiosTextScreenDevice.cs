// Copyright (c) Lonos Project. All rights reserved.
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

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
