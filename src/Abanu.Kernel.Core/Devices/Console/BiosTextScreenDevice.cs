// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System;

namespace Abanu.Kernel.Core.Devices
{

    /// <summary>
    /// Text stream for BIOS Text Screen
    /// </summary>
    public class BiosTextScreenDevice : IBuffer
    {

        public BiosTextScreenDevice()
        {
        }

        /// <summary>
        /// Reading is not supported
        /// </summary>
        public unsafe SSize Read(byte* buf, USize count)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Writes Text to the BIOS Text Screen.
        /// The underlining Device is responsible for wrapping and scrolling
        /// </summary>
        public unsafe SSize Write(byte* buf, USize count)
        {
            for (var i = 0; i < count; i++)
                Screen.Write((char)buf[i]);

            return (uint)count;
        }

    }

}
