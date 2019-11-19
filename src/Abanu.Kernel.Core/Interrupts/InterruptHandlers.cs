// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System;
using Abanu.Kernel.Core.Diagnostics;
using Abanu.Kernel.Core.MemoryManagement;
using Abanu.Kernel.Core.Processes;
using Abanu.Kernel.Core.Scheduling;
using Abanu.Kernel.Core.SysCalls;
using Mosa.Runtime;
using Mosa.Runtime.x86;

namespace Abanu.Kernel.Core.Interrupts
{

    internal static class InterruptHandlers
    {

        private static void Error(ref IDTStack stack, string message)
        {
            Panic.ESP = stack.ESP;
            Panic.EBP = stack.EBP;
            Panic.EIP = stack.EIP;
            Panic.EAX = stack.EAX;
            Panic.EBX = stack.EBX;
            Panic.ECX = stack.ECX;
            Panic.EDX = stack.EDX;
            Panic.EDI = stack.EDI;
            Panic.ESI = stack.ESI;
            Panic.CS = stack.CS;
            Panic.ErrorCode = stack.ErrorCode;
            Panic.EFLAGS = stack.EFLAGS;
            Panic.Interrupt = stack.Interrupt;
            Panic.CR2 = Native.GetCR2();
            Panic.FS = Native.GetFS();
            Panic.Error(message);
        }

        internal static void Undefined(ref IDTStack stack)
        {
            var handler = IDTManager.Handlers[stack.Interrupt];
            if (handler.NotifyUnhandled)
            {
                handler.NotifyUnhandled = false;
                KernelMessage.WriteLine("Unhandled Interrupt {0}", stack.Interrupt);
            }
        }

        internal static void Service(ref IDTStack stack)
        {
            var handler = IDTManager.Handlers[stack.Interrupt];
            if (handler.Service == null)
            {
                KernelMessage.WriteLine("handler.Service == null");
                return;
            }

            var ctx = new SysCallContext
            {
                CallingType = SysCallCallingType.Async, // Important! Otherwise stack will corrupted
            };

            var msg = new SystemMessage(SysCallTarget.Interrupt)
            {
                Arg1 = stack.Interrupt,
            };

            Scheduler.SaveThreadState(Scheduler.GetCurrentThread().ThreadID, ref stack);
            handler.Service.SwitchToThreadMethod(ref ctx, ref msg);
        }

        /// <summary>
        /// Interrupt 0
        /// </summary>
        internal static void DivideError(ref IDTStack stack)
        {
            Error(ref stack, "Divide Error");
        }

        /// <summary>
        /// Interrupt 4
        /// </summary>
        internal static void ArithmeticOverflowException(ref IDTStack stack)
        {
            Error(ref stack, "Arithmetic Overflow Exception");
        }

        /// <summary>
        /// Interrupt 5
        /// </summary>
        internal static void BoundCheckError(ref IDTStack stack)
        {
            Error(ref stack, "Bound Check Error");
        }

        /// <summary>
        /// Interrupt 6
        /// </summary>
        internal static void InvalidOpcode(ref IDTStack stack)
        {
            Error(ref stack, "Invalid Opcode");
        }

        /// <summary>
        /// Interrupt 7
        /// </summary>
        internal static void CoProcessorNotAvailable(ref IDTStack stack)
        {
            Error(ref stack, "Co-processor Not Available");
        }

        /// <summary>
        /// Interrupt 8
        /// </summary>
        internal static void DoubleFault(ref IDTStack stack)
        {
            //TODO: Analyze Double Fault
            Error(ref stack, "Double Fault");
        }

        /// <summary>
        /// Interrupt 9
        /// </summary>
        internal static void CoProcessorSegmentOverrun(ref IDTStack stack)
        {
            Error(ref stack, "Co-processor Segment Overrun");
        }

        /// <summary>
        /// Interrupt 10
        /// </summary>
        internal static void InvalidTSS(ref IDTStack stack)
        {
            Error(ref stack, "Invalid TSS");
        }

        /// <summary>
        /// Interrupt 11
        /// </summary>
        internal static void SegmentNotPresent(ref IDTStack stack)
        {
            Error(ref stack, "Segment Not Present");
        }

        /// <summary>
        /// Interrupt 12
        /// </summary>
        internal static void StackException(ref IDTStack stack)
        {
            Error(ref stack, "Stack Exception");
        }

        /// <summary>
        /// Interrupt 13
        /// </summary>
        internal static void GeneralProtectionException(ref IDTStack stack)
        {
            Error(ref stack, "General Protection Exception");
        }

        /// <summary>
        /// Interrupt 14
        /// </summary>
        internal static void PageFault(ref IDTStack stack)
        {
            // Check if Null Pointer Exception
            // Otherwise handle as Page Fault

            var cr2 = Native.GetCR2();

            if ((cr2 >> 5) < 0x1000)
            {
                Error(ref stack, "Null Pointer Exception");
            }

            if (cr2 >= 0xF0000000u)
            {
                Error(ref stack, "Invalid Access Above 0xF0000000");
                return;
            }

            Error(ref stack, "Not mapped");

            // var physicalpage = PageFrameManager.AllocatePage(PageFrameRequestFlags.Default);

            // if (physicalpage == null)
            // {
            //     Error(ref stack, "Out of Memory");
            //     return;
            // }

            // PageTable.MapVirtualAddressToPhysical(cr2, (uint)physicalpage);
        }

        /// <summary>
        /// Interrupt 16
        /// </summary>
        internal static void CoProcessorError(ref IDTStack stack)
        {
            Error(ref stack, "Co-Processor Error");
        }

        /// <summary>
        /// Interrupt 19
        /// </summary>
        internal static void SIMDFloatinPointException(ref IDTStack stack)
        {
            Error(ref stack, "SIMD Floating-Point Exception");
        }

        /// <summary>
        /// Interrupt 32
        /// </summary>
        internal static void ClockTimer(ref IDTStack stack)
        {
            //Screen.Goto(15, 5);
            //Screen.Write(IDTManager.RaisedCount);

            // to clock events...

            // at least, call scheduler. Keep in mind, it may not return because of thread switching
            Scheduler.ClockInterrupt(ref stack);
        }

        internal static void Keyboard(ref IDTStack stack)
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
                case 0x3B:
                    KeyF1();
                    break;
                case 0x3C:
                    KeyF2();
                    break;
                case 0x3D:
                    KeyF3();
                    break;
                case 0x3E:
                    KeyF4();
                    break;
                case 0x3F:
                    KeyF5();
                    break;
                case 0x40:
                    KeyF6();
                    break;
                case 0x41:
                    KeyF7();
                    break;
                case 0x42:
                    KeyF8();
                    break;
                case 0x43:
                    KeyF9();
                    break;
                case 0x44:
                    KeyF10();
                    break;
                case 0x57:
                    KeyF11();
                    break;
                case 0x58:
                    KeyF12();
                    break;
            }
        }

        internal static void TermindateCurrentThread(ref IDTStack stack)
        {
            Scheduler.TerminateCurrentThread();
        }

        private static void Key1()
        {
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

        private static void KeyF1()
        {
            Scheduler.DumpStats();
        }

        private static void KeyF2()
        {
            ProcessManager.DumpStats();
        }

        private static void KeyF3()
        {
            //PhysicalPageManager.DumpPages();
            PhysicalPageManager.DumpStats();
            VirtualPageManager.DumpStats();
        }

        private static void KeyF4()
        {
        }

        private static void KeyF5()
        {
            Screen.Clear();
        }

        private static void KeyF6()
        {
        }

        private static void KeyF7()
        {
        }

        private static void KeyF8()
        {
        }

        private static void KeyF9()
        {
        }

        private static void KeyF10()
        {
        }

        private static void KeyF11()
        {
        }

        private static void KeyF12()
        {
        }

    }
}
