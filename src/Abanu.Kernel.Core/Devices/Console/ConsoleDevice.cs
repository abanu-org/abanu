// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System;

namespace Abanu.Kernel.Core.Devices
{

    /// <summary>
    /// Generic wrapper for console output
    /// </summary>
    public class ConsoleDevice : IBuffer
    {

        private IBuffer Device;

        public ConsoleDevice(IBuffer device)
        {
            Device = device;
        }

        /// <summary>
        /// Sets a new output target
        /// </summary>
        public void SetOutputDevice(IBuffer device)
        {
            Device = device;
        }

        /// <summary>
        /// Writes text to the underlining target
        /// </summary>
        public unsafe SSize Read(byte* buf, USize count)
        {
            return Device.Read(buf, count);
        }

        public unsafe SSize Write(byte* buf, USize count)
        {
            return Device.Write(buf, count);
        }

    }

}
