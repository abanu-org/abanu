// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using Mosa.Runtime.x86;

namespace Abanu.Kernel.Core.Interrupts
{
    /// <summary>
    /// Programmable Interrupt Controller (PIC)
    /// </summary>
    public static class PIC
    {
        private const byte ICW1_ICW4 = 0x01;
        private const byte ICW1_SingleCascadeMode = 0x02;
        private const byte ICW1_Interval4 = 0x04;
        private const byte ICW1_LevelTriggeredEdgeMode = 0x08;
        private const byte ICW1_INIT = 0x10;

        // Offsets needs be >=20, so it does not conflict with CPU Exceptions interrupts
        // Offsets needs to be divisible by 8.
        private const byte ICW2_MasterOffset = 0x20;
        private const byte ICW2_SlaveOffset = 0x28;

        private const byte ICW4_8086 = 0x01;
        private const byte ICW4_AutoEndOfInterrupt = 0x02;
        private const byte ICW4_BufferedSlaveMode = 0x08;
        private const byte ICW4_BufferedMasterMode = 0x0C;
        private const byte ICW4_SpecialFullyNested = 0x10;

        private const byte PIC1 = 0x20;
        private const byte PIC2 = 0xA0;

        private const byte PIC1_COMMAND = PIC1;
        private const byte PIC2_COMMAND = PIC2;
        private const byte PIC1_DATA = PIC1 + 1;
        private const byte PIC2_DATA = PIC2 + 1;

        private const byte EOI = 0x20;

        public static void Setup()
        {
            KernelMessage.WriteLine("Setup PIC");
            byte masterMask = Native.In8(PIC1_DATA);
            byte slaveMask = Native.In8(PIC2_DATA);

            // ICW1 - Set Initialize Controller & Expect ICW4
            Native.Out8(PIC1_COMMAND, ICW1_INIT | ICW1_ICW4);

            // ICW2 - interrupt offset
            Native.Out8(PIC1_DATA, ICW2_MasterOffset);

            // ICW3
            Native.Out8(PIC1_DATA, 4);

            // ICW4 - Set 8086 Mode
            Native.Out8(PIC1_DATA, ICW4_8086);

            // OCW1
            Native.Out8(PIC1_DATA, masterMask);

            // ICW1 - Set Initialize Controller & Expect ICW4
            Native.Out8(PIC2_COMMAND, ICW1_INIT | ICW1_ICW4);

            // ICW2 - interrupt offset
            Native.Out8(PIC2_DATA, ICW2_SlaveOffset);

            // ICW3
            Native.Out8(PIC2_DATA, 2);

            // ICW4 - Set 8086 Mode
            Native.Out8(PIC2_DATA, ICW4_8086);

            // OCW1
            Native.Out8(PIC2_DATA, slaveMask);
        }

        /// <summary>
        /// Sends the end of interrupt.
        /// </summary>
        /// <param name="irq">The IRQ.</param>
        public static void SendEndOfInterrupt(uint irq)
        {
            if (irq >= ICW2_SlaveOffset) // or untranslated: IRQ >= 8
                Native.Out8(PIC2_COMMAND, EOI);

            Native.Out8(PIC1_COMMAND, EOI);
        }
    }
}
