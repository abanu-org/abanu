// Copyright (c) MOSA Project. Licensed under the New BSD License.

using Mosa.Runtime;
using Mosa.Runtime.x86;
using System;

using lonos.Kernel.Core;
using lonos.Kernel.Core.Interrupts;
using lonos.Kernel.Core.Diagnostics;

//TODO: Name in compiler
namespace Mosa.Kernel.x86
{

    /// <summary>
    /// IDT
    /// </summary>
    public unsafe static class IDT
    {


        public static bool Enabled = false;
        private static void dummy(uint err) { }
        /// <summary>
        /// Interrupts the handler.
        /// </summary>
        /// <param name="stackStatePointer">The stack state pointer.</param>
        private unsafe static void ProcessInterrupt(uint stackStatePointer)
        {
            Native.SetSegments(0x10, 0x10, 0x10, 0x10, 0x10);

            //KernelMessage.WriteLine("Interrupt occured");

            var stack = (IDTStack*)stackStatePointer;
            var irq = stack->Interrupt;

            if (irq == (uint)KnownInterrupt.PageFault)
            {
                var errorCode = stack->ErrorCode;
                dummy(errorCode);
            }

            if (!Enabled)
            {
                PIC.SendEndOfInterrupt(irq);
                return;
            }

            if (irq != (uint)KnownInterrupt.ClockTimer)
            {
                dummy(irq);
            }

            var interruptInfo = IDTManager.handlers[irq];

            IDTManager.RaisedCount++;

            if (interruptInfo.CountStatistcs)
                IDTManager.RaisedCountCustom++;
            if (interruptInfo.Trace)
                KernelMessage.WriteLine("Interrupt: {0}", irq);

            var col = Screen.column;
            var row = Screen.row;
            Screen.column = 0;
            Screen.Goto(2, 35);
            Screen.Write("Interrupts: ");
            Screen.Write(IDTManager.RaisedCount);
            Screen.Goto(3, 35);
            Screen.Write("IntNoClock: ");
            Screen.Write(IDTManager.RaisedCountCustom);
            Screen.row = row;
            Screen.column = col;

            if (irq < 0 || irq > 255)
                Panic.Error("Invalid Interrupt");

            if (interruptInfo.Handler == null)
            {
                Panic.Error("Handler is null");
            }
            else
            {

            }

            interruptInfo.Handler(stack);

            PIC.SendEndOfInterrupt(irq);
        }

    }

}
