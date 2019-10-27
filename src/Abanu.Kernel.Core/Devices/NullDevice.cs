// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;

namespace Abanu.Kernel.Core.Devices
{

    public class NullDevice : IBuffer
    {

        public NullDevice()
        {
        }

        public unsafe SSize Read(byte* buf, USize count)
        {
            return 0;
        }

        public unsafe SSize Write(byte* buf, USize count)
        {
            return (uint)count;
        }

    }

}
