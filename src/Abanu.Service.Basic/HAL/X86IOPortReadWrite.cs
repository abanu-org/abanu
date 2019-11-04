// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using Mosa.DeviceSystem;

namespace Abanu.Kernel
{
    /// <summary>
    /// X86IOPortReadWrite
    /// </summary>
    /// <seealso cref="Mosa.DeviceSystem.BaseIOPortReadWrite" />
    public sealed class X86IOPortReadWrite : BaseIOPortReadWrite
    {
        public X86IOPortReadWrite(ushort address)
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

        /// <summary>
        ///  Writes a byte to the IO Port
        /// </summary>
        /// <param name="data">The data.</param>
        public override void Write8(byte data)
        {
            IOPort.Out8(Address, data);
        }

        /// <summary>
        ///  Writes a short to the IO Port
        /// </summary>
        /// <param name="data">The data.</param>
        public override void Write16(ushort data)
        {
            IOPort.Out16(Address, data);
        }

        /// <summary>
        ///  Writes an integer to the IO Port
        /// </summary>
        /// <param name="data">The data.</param>
        public override void Write32(uint data)
        {
            IOPort.Out32(Address, data);
        }
    }
}
