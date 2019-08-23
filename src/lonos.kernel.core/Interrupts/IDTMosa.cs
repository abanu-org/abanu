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

        public static bool Enabled = false;
        private static void test(uint err) { }
        /// <summary>
        /// Interrupts the handler.
        /// </summary>
        /// <param name="stackStatePointer">The stack state pointer.</param>
        private unsafe static void ProcessInterrupt(uint stackStatePointer)
        {
            //KernelMessage.WriteLine("Interrupt occured");

            var stack = (IDTStack*)stackStatePointer;
            var irq = stack->Interrupt;

            if (irq == 14)
            {
                var errorCode = stack->ErrorCode;
                test(errorCode);
            }

            if (!Enabled)
                return;

            var info = IDTManager.handlers[irq];

            IDTManager.RaisedCount++;

            if (info.CountStatistcs)
                IDTManager.RaisedCountCustom++;
            if (info.Trace)
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

            if (info.Handler == null)
            {
                //Panic.Error("Handlr is null");
            }
            else
            {

            }

            info.Handler(stack);

            PIC.SendEndOfInterrupt(irq);
        }

    }

}
