// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System;
using Abanu.Kernel.Core;
using Abanu.Kernel.Core.Diagnostics;
using Abanu.Kernel.Core.Interrupts;
using Abanu.Kernel.Core.PageManagement;
using Abanu.Kernel.Core.Scheduling;
using Mosa.Runtime;
using Mosa.Runtime.x86;

namespace Abanu.Kernel.Core
{

    /// <summary>
    /// Interrupt Descriptor Table
    /// </summary>
    public static unsafe class IDT
    {

        /// <summary>
        /// Entry point into the ISR (Interrupt Service Routine)
        /// </summary>
        /// <param name="stackStatePointer">Pointer to the ISR stack</param>
        private static unsafe void ProcessInterrupt(uint stackStatePointer)
        {
            // Switch to Kernel segments
            ushort dataSelector = 0x10;
            Native.SetSegments(dataSelector, dataSelector, dataSelector, dataSelector, dataSelector);

            // Switch to Kernel Adresse space
            var block = (InterruptControlBlock*)Address.InterruptControlBlock;
            Native.SetCR3(block->KernelPageTableAddr);

            // Get the IRQ
            var stack = (IDTStack*)stackStatePointer;
            var irq = stack->Interrupt;

            // Get the pagetable address of the interrupted process
            uint pageTableAddr = 0;
            var thread = Scheduler.GetCurrentThread();
            if (thread != null)
            {
                dataSelector = (ushort)thread.DataSelector;
                pageTableAddr = thread.Process.PageTable.GetPageTablePhysAddr();
            }

            // If the IDTManager is not initialized yet or hard disabled, we return now
            if (!IDTManager.Enabled)
            {
                PIC.SendEndOfInterrupt(irq);
                return;
            }

            // Get interrupt info for the IRQ
            var interruptInfo = IDTManager.Handlers[irq];
            if (KConfig.Log.Interrupts && interruptInfo.Trace && thread != null)
                KernelMessage.WriteLine("Interrupt {0}, Thread {1}, EIP={2:X8} ESP={3:X8}", irq, (uint)thread.ThreadID, stack->EIP, stack->ESP);

            // Some statistics

            IDTManager.RaisedCount++;

            if (interruptInfo.CountStatistcs)
                IDTManager.RaisedCountCustom++;

            if (KConfig.Log.Interrupts)
            {
                if (interruptInfo.Trace)
                    KernelMessage.WriteLine("Interrupt: {0}", irq);

                var col = Screen.Column;
                var row = Screen.Row;
                Screen.Column = 0;
                Screen.Goto(2, 35);
                Screen.Write("Interrupts: ");
                Screen.Write(IDTManager.RaisedCount);
                Screen.Goto(3, 35);
                Screen.Write("IntNoClock: ");
                Screen.Write(IDTManager.RaisedCountCustom);
                Screen.Row = row;
                Screen.Column = col;
            }

            // This should never happen
            if (irq < 0 || irq > 255)
                Panic.Error("Invalid Interrupt");

            // Invoke handlers

            if (interruptInfo.PreHandler != null)
                interruptInfo.PreHandler(stack);

            if (interruptInfo.Handler == null)
            {
                Panic.Error("Handler is null");
            }
            else
            {

            }

            interruptInfo.Handler(stack);

            // Important! Otherwise we will get any more interrupts of this kind
            PIC.SendEndOfInterrupt(irq);

            // Switch to original address space
            if (pageTableAddr > 0)
                Native.SetCR3(pageTableAddr);

            // Switch to original segments
            Native.SetSegments(dataSelector, dataSelector, dataSelector, dataSelector, 0x10);

            // ISR is completed. The upper ISR stub will re-enable interrupts and resume the original process
        }

    }

}
