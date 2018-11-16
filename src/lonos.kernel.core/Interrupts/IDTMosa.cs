// Copyright (c) MOSA Project. Licensed under the New BSD License.

using Mosa.Runtime;
using Mosa.Runtime.x86;
using System;

using lonos.kernel.core;

//TODO: Name in compiler
namespace Mosa.Kernel.x86
{

    /// <summary>
    /// IDT
    /// </summary>
    public unsafe static class IDT
    {

        /// <summary>
        /// Interrupts the handler.
        /// </summary>
        /// <param name="stackStatePointer">The stack state pointer.</param>
        private unsafe static void ProcessInterrupt(uint stackStatePointer)
        {
            IDTManager.RaisedCount++;

            var stack = (IDTStack*)stackStatePointer;
            var irq = stack->Interrupt;
            if (irq < 0 || irq > 255)
                Panic.Error("Invalid Interrupt");

            var handler = IDTManager.handlers[irq];
            if (handler == null)
                Panic.Error("Handlr is null");

            handler(stack);

            PIC.SendEndOfInterrupt(irq);
        }

    }

}
