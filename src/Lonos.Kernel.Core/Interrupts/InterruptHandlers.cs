// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using Lonos.Kernel.Core.Diagnostics;
using Lonos.Kernel.Core.MemoryManagement;
using Lonos.Kernel.Core.Scheduling;
using Lonos.Kernel.Core.SysCalls;
using Mosa.Runtime;
using Mosa.Runtime.x86;

namespace Lonos.Kernel.Core.Interrupts
{

    public static unsafe class InterruptHandlers
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

        public static void Undefined(IDTStack* stack)
        {
            var handler = IDTManager.Handlers[stack->Interrupt];
            if (handler.NotifyUnhandled)
            {
                handler.NotifyUnhandled = false;
                KernelMessage.WriteLine("Unhandled Interrupt {0}", stack->Interrupt);
            }
        }

        public static void Service(IDTStack* stack)
        {
            var handler = IDTManager.Handlers[stack->Interrupt];
            if (handler.Service == null)
            {
                KernelMessage.WriteLine("handler.Service == null");
                return;
            }

            var msg = new SystemMessage(SysCallTarget.Interrupt)
            {
                Arg1 = stack->Interrupt,
            };

            Scheduler.SaveThreadState(Scheduler.GetCurrentThread().ThreadID, (IntPtr)stack);
            handler.Service.SwitchToThreadMethod(&msg, false);
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
            Error(stack, "Stack Exception");
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

            Error(stack, "Not mapped");

            // var physicalpage = PageFrameManager.AllocatePage(PageFrameRequestFlags.Default);

            // if (physicalpage == null)
            // {
            //     Error(stack, "Out of Memory");
            //     return;
            // }

            // PageTable.MapVirtualAddressToPhysical(cr2, (uint)physicalpage);
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

            // to clock events...

            // at least, call scheduler. Keep in mind, it may not return because of thread switching
            Scheduler.ClockInterrupt(new IntPtr(stack));
        }

        public static void Keyboard(IDTStack* stack)
        {
            //Screen.Goto(15, 5);
            //Screen.Write(IDTManager.RaisedCount);
            var code = (uint)Native.In8(0x60);
            //KernelMessage.WriteLine("Got Keyboard scancode: {0:X2}", code);

            // for debugging
            switch (code)
            {
                case 0x02:
                    Key1();
                    break;
                case 0x03:
                    Key2();
                    break;
                case 0x04:
                    Key3();
                    break;
                case 0x05:
                    Key4();
                    break;
                case 0x06:
                    Key5();
                    break;
                case 0x07:
                    Key6();
                    break;
                case 0x57:
                    KeyF11();
                    break;
                case 0x58:
                    KeyF12();
                    break;
            }
        }

        public static void TermindateCurrentThread(IDTStack* stack)
        {
            Scheduler.TerminateCurrentThread();
        }

        private static void Key1()
        {
            Screen.Clear();
        }

        private static void Key2()
        {
        }

        private static void Key3()
        {
        }

        private static void Key4()
        {
        }

        private static void Key5()
        {
        }

        private static void Key6()
        {
        }

        private static void KeyF11()
        {
        }

        private static void KeyF12()
        {
            Scheduler.Dump();
        }

    }
}
