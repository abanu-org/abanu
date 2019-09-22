// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;

namespace Lonos.Kernel.Core.Devices
{

    public class ConsoleDevice : IBuffer
    {

        private IBuffer Device;

        public ConsoleDevice(IBuffer device)
        {
            Device = device;
        }

        public unsafe SSize Read(byte* buf, USize count)
        {
            return Device.Read(buf, count);
        }

        public void SetOutputDevice(IBuffer device)
        {
            Device = device;
        }

        public unsafe SSize Write(byte* buf, USize count)
        {
            return Device.Write(buf, count);
        }

    }

}
