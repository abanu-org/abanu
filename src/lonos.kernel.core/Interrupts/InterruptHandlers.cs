// Copyright (c) MOSA Project. Licensed under the New BSD License.

using Mosa.Runtime;
using Mosa.Runtime.x86;
using System;

namespace lonos.kernel.core
{

    public unsafe static class InterruptsHandlers
    {

        private static void Error(IDTStack* stack, string message)
        {
            Panic.ESP = stack->ESP;
            Panic.EBP = stack->EBP;
            Panic.EIP = stack->EIP;
            Panic.EAX = stack->EAX;
            Panic.EBX = stack->EBX;
            Panic.ECX = stack->ECX;
            Panic.EDX = stack->EDX;
            Panic.EDI = stack->EDI;
            Panic.ESI = stack->ESI;
            Panic.CS = stack->CS;
            Panic.ErrorCode = stack->ErrorCode;
            Panic.EFLAGS = stack->EFLAGS;
            Panic.Interrupt = stack->Interrupt;
            Panic.CR2 = Native.GetCR2();
            Panic.FS = Native.GetFS();
            Panic.Error(message);
        }

        /// <summary>
        /// Interrupt 0
        /// </summary>
        public static void DivideError(IDTStack* stack)
        {
            Error(stack, "Divide Error");
        }

        /// <summary>
        /// Interrupt 4
        /// </summary>
        public static void ArithmeticOverflowException(IDTStack* stack)
        {
            Error(stack, "Arithmetic Overflow Exception");
        }

        /// <summary>
        /// Interrupt 5
        /// </summary>
        public static void BoundCheckError(IDTStack* stack)
        {
            Error(stack, "Bound Check Error");
        }

        /// <summary>
        /// Interrupt 6
        /// </summary>
        public static void InvalidOpcode(IDTStack* stack)
        {
            Error(stack, "Invalid Opcode");
        }

        /// <summary>
        /// Interrupt 7
        /// </summary>
        public static void CoProcessorNotAvailable(IDTStack* stack)
        {
            Error(stack, "Co-processor Not Available");
        }

        /// <summary>
        /// Interrupt 8
        /// </summary>
        public static void DoubleFault(IDTStack* stack)
        {
            //TODO: Analyze Double Fault
            Error(stack, "Double Fault");
        }

        /// <summary>
        /// Interrupt 9
        /// </summary>
        public static void CoProcessorSegmentOverrun(IDTStack* stack)
        {
            Error(stack, "Co-processor Segment Overrun");
        }

        /// <summary>
        /// Interrupt 10
        /// </summary>
        public static void InvalidTSS(IDTStack* stack)
        {
            Error(stack, "Invalid TSS");
        }

        /// <summary>
        /// Interrupt 11
        /// </summary>
        public static void SegmentNotPresent(IDTStack* stack)
        {
            Error(stack, "Segment Not Present");
        }

        /// <summary>
        /// Interrupt 12
        /// </summary>
        public static void StackException(IDTStack* stack)
        {
            Error(stack, "Stack Excetion");
        }

        /// <summary>
        /// Interrupt 13
        /// </summary>
        public static void GeneralProtectionException(IDTStack* stack)
        {
            Error(stack, "General Protection Exception");
        }

        /// <summary>
        /// Interrupt 14
        /// </summary>
        public static void PageFault(IDTStack* stack)
        {
            // Check if Null Pointer Exception
            // Otherwise handle as Page Fault

            var cr2 = Native.GetCR2();

            if ((cr2 >> 5) < 0x1000)
            {
                Error(stack, "Null Pointer Exception");
            }

            if (cr2 >= 0xF0000000u)
            {
                Error(stack, "Invalid Access Above 0xF0000000");
                return;
            }

            var physicalpage = PageFrameAllocator.Allocate();

            if (physicalpage == Addr.Invalid)
            {
                Error(stack, "Out of Memory");
                return;
            }

            PageTable.MapVirtualAddressToPhysical(cr2, (uint)physicalpage);
        }

        /// <summary>
        /// Interrupt 16
        /// </summary>
        public static void CoProcessorError(IDTStack* stack)
        {
            Error(stack, "Co-Processor Error");
        }

        /// <summary>
        /// Interrupt 19
        /// </summary>
        public static void SIMDFloatinPointException(IDTStack* stack)
        {
            Error(stack, "SIMD Floating-Point Exception");
        }

        /// <summary>
        /// Interrupt 32
        /// </summary>
        public static void ClockTimer(IDTStack* stack)
        {
            //Screen.Goto(15, 5);
            //Screen.Write(IDTManager.RaisedCount);
        }

    }
}
