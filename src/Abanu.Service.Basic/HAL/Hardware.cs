// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System;
using Abanu;
using Abanu.Runtime;
using Mosa.DeviceSystem;
using Mosa.Runtime;
using Mosa.Runtime.x86;

namespace Abanu.Kernel
{
    /// <summary>
    /// Hardware
    /// </summary>
    public sealed class Hardware : BaseHardwareAbstraction
    {
        /// <summary>
        /// Gets the size of the page.
        /// </summary>
        public override uint PageSize => 4096;

        /// <summary>
        /// Gets a block of memory from the kernel
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="size">The size.</param>
        public override ConstrainedPointer GetPhysicalMemory(Pointer address, uint size)
        {
            var virtAddr = (Addr)SysCalls.GetPhysicalMemory((uint)address, size);

            return new ConstrainedPointer((Pointer)(uint)virtAddr, size);
        }

        /// <summary>
        /// Disables all interrupts.
        /// </summary>
        public override void DisableAllInterrupts()
        {
            Native.Cli();
        }

        /// <summary>
        /// Enables all interrupts.
        /// </summary>
        public override void EnableAllInterrupts()
        {
            Native.Sti();
        }

        /// <summary>
        /// Processes the interrupt.
        /// </summary>
        /// <param name="irq">The IRQ.</param>
        public override void ProcessInterrupt(byte irq)
        {
            HAL.ProcessInterrupt(irq);
        }

        /// <summary>
        /// Sleeps the specified milliseconds.
        /// </summary>
        /// <param name="milliseconds">The milliseconds.</param>
        public override void Sleep(uint milliseconds)
        {
        }

        /// <summary>
        /// Allocates the virtual memory.
        /// </summary>
        public override ConstrainedPointer AllocateVirtualMemory(uint size, uint alignment)
        {
            var address = (IntPtr)SysCalls.RequestMemory(size);

            return new ConstrainedPointer((Pointer)(uint)address, size);
        }

        /// <summary>
        /// Gets the physical address.
        /// </summary>
        public override Pointer TranslateVirtualToPhysicalAddress(Pointer virtualAddress)
        {
            return (Pointer)SysCalls.TranslateVirtualToPhysicalAddress((uint)virtualAddress);
        }

        /// <summary>
        /// Requests an IO read/write port interface from the kernel
        /// </summary>
        /// <param name="port">The port number.</param>
        public override BaseIOPortReadWrite GetReadWriteIOPort(ushort port)
        {
            return new X86IOPortReadWrite(port);
        }

        /// <summary>
        /// Requests an IO read/write port interface from the kernel
        /// </summary>
        /// <param name="port">The port number.</param>
        public override BaseIOPortRead GetReadIOPort(ushort port)
        {
            return new X86IOPortReadWrite(port);
        }

        /// <summary>
        /// Requests an IO write port interface from the kernel
        /// </summary>
        /// <param name="port">The port number.</param>
        public override BaseIOPortWrite GetWriteIOPort(ushort port)
        {
            return new X86IOPortWrite(port);
        }

        /// <summary>
        /// Debugs the write.
        /// </summary>
        /// <param name="message">The message.</param>
        public override void DebugWrite(string message)
        {
            //Boot.Console.Write(message);
        }

        /// <summary>
        /// Debugs the write line.
        /// </summary>
        /// <param name="message">The message.</param>
        public override void DebugWriteLine(string message)
        {
            //Boot.Console.WriteLine(message);
        }

        /// <summary>
        /// Aborts with the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        public override void Abort(string message)
        {
            //Panic.Error(message);
        }
    }
}
