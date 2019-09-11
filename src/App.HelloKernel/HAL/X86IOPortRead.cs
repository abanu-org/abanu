// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using Mosa.DeviceSystem;

namespace Lonos.Kernel
{
    /// <summary>
    /// X86IOPortRead
    /// </summary>
    /// <seealso cref="Mosa.DeviceSystem.BaseIOPortRead" />
    public sealed class X86IOPortRead : BaseIOPortRead
    {
        public X86IOPortRead(ushort address)
        {
            Address = address;
        }

        /// <summary>
        /// Reads a byte from the IO Port
        /// </summary>
        public override byte Read8()
        {
            return IOPort.In8(Address);
        }

        /// <summary>
        /// Reads a short from the IO Port
        /// </summary>
        public override ushort Read16()
        {
            return IOPort.In16(Address);
        }

        /// <summary>
        /// Reads an integer from the IO Port
        /// </summary>
        public override uint Read32()
        {
            return IOPort.In32(Address);
        }
    }
}
