// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;

namespace Lonos.Kernel.Core.Devices
{

    public class ConsoleDevice : IFile
    {

        private IFile Device;

        public ConsoleDevice(IFile device)
        {
            Device = device;
        }

        public void SetOutputDevice(IFile device)
        {
            Device = device;
        }

        public unsafe SSize Write(byte* buf, USize count)
        {
            return Device.Write(buf, count);
        }

    }

}
