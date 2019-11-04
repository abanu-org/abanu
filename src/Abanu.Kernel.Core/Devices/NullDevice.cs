// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;

namespace Abanu.Kernel.Core.Devices
{

    /// <summary>
    /// This device will always return 0. Written bytes will written nowhere.
    /// </summary>
    public class NullDevice : IBuffer
    {

        public NullDevice()
        {
        }

        /// <summary>
        /// Returns always 0
        /// </summary>
        public unsafe SSize Read(byte* buf, USize count)
        {
            return 0;
        }

        /// <summary>
        /// Written bytes will written nowhere
        /// </summary>
        public unsafe SSize Write(byte* buf, USize count)
        {
            return (uint)count;
        }

    }

}
