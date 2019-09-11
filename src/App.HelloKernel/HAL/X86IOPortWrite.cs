// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using Mosa.DeviceSystem;

namespace Lonos.Kernel
{

    /// <summary>
    /// X86IOPortWrite
    /// </summary>
    /// <seealso cref="Mosa.DeviceSystem.BaseIOPortWrite" />
    public sealed class X86IOPortWrite : BaseIOPortWrite
    {
        public X86IOPortWrite(ushort address)
        {
            Address = address;
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
        /// Writes a short to the IO Port
        /// </summary>
        /// <param name="data">The data.</param>
        public override void Write16(ushort data)
        {
            IOPort.Out16(Address, data);
        }

        /// <summary>
        /// Writes an integer to the IO Port
        /// </summary>
        /// <param name="data">The data.</param>
        public override void Write32(uint data)
        {
            IOPort.Out32(Address, data);
        }
    }
}
