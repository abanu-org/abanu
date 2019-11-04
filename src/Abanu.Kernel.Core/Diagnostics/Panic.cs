// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using Abanu.Kernel.Core.MemoryManagement;
using Abanu.Kernel.Core.Processes;
using Abanu.Kernel.Core.Scheduling;
using Mosa.Runtime;
using Mosa.Runtime.x86;

namespace Abanu.Kernel.Core.Diagnostics
{
    /// <summary>
    /// Kernel Panic
    /// </summary>
    public static class Panic
    {
        private static bool firstError = true;

        public static uint EBP = 0;
        public static uint EIP = 0;
        public static uint EAX = 0;
        public static uint EBX = 0;
        public static uint ECX = 0;
        public static uint EDX = 0;
        public static uint EDI = 0;
        public static uint ESI = 0;
        public static uint ESP = 0;
        public static uint Interrupt = 0;
        public static uint ErrorCode = 0;
        public static uint CS = 0;
        public static X86_EFlags EFLAGS = 0;
        public static uint CR2 = 0;
        public static uint FS = 0;

        /// <summary>
        /// Prints the error and halts the system
        /// </summary>
        public static void Error(string message)
        {
            Native.Cli();

            //Screen.Goto(0, 0);
            //Screen.Color = 11;
            //Screen.Write(message);
            //while(true){ Native.Nop(); };
            //return;
            Screen.BackgroundColor = ScreenColor.Green;

            Screen.Clear();
            Screen.Goto(1, 0);
            Screen.Color = ScreenColor.White;
            Screen.Write("*** Kernel Panic ***");

            if (firstError)
                firstError = false;
            else
                Screen.Write(" (multiple)");

            Screen.NextLine();
            Screen.NextLine();
            Screen.Write(message);
            Screen.NextLine();
            Screen.NextLine();
            Screen.Write("REGISTERS:");
            Screen.NextLine();
            Screen.NextLine();
            DumpRegisters();
            Screen.NextLine();
            Screen.Write("STACK TRACE:");
            Screen.NextLine();
            Screen.NextLine();
            DumpStackTrace();

            KernelMessage.WriteLine("Kernel Panic: {0}", message);

            while (true)
            {
                // keep debugger running
                unsafe
                {
                    //Debugger.Process(null);
                }

                Native.Hlt();
            }
        }

        private static void DumpRegisters()
        {
            Screen.Write("EIP: ");
            Screen.Write(EIP, 16, 8);
            Screen.Write(" ESP: ");
            Screen.Write(ESP, 16, 8);
            Screen.Write(" EBP: ");
            Screen.Write(EBP, 16, 8);
            Screen.Write(" EFLAGS: ");
            Screen.Write((uint)EFLAGS, 16, 8);
            Screen.Write(" CR2: ");
            Screen.Write(CR2, 16, 8);
            Screen.NextLine();
            Screen.Write("EAX: ");
            Screen.Write(EAX, 16, 8);
            Screen.Write(" EBX: ");
            Screen.Write(EBX, 16, 8);
            Screen.Write(" ECX: ");
            Screen.Write(ECX, 16, 8);
            Screen.Write(" CS: ");
            Screen.Write(CS, 16, 8);
            Screen.Write(" FS: ");
            Screen.Write(FS, 16, 8);
            Screen.NextLine();
            Screen.Write("EDX: ");
            Screen.Write(EDX, 16, 8);
            Screen.Write(" EDI: ");
            Screen.Write(EDI, 16, 8);
            Screen.Write(" ESI: ");
            Screen.Write(ESI, 16, 8);
            Screen.Write(" ERROR: ");
            Screen.Write(ErrorCode, 16, 2);
            Screen.Write(" IRQ: ");
            Screen.Write(Interrupt, 16, 2);
            Screen.NextLine();
        }

        private static void DumpStackTrace()
        {
            DumpStackTrace(3);
        }

        private static void DumpStackTrace(uint depth)
        {
            //return;
            while (true)
            {
                var entry = Mosa.Runtime.Internal.GetStackTraceEntry(depth, new Pointer(EBP), new Pointer(EIP));

                if (!entry.Valid)
                    return;

                if (!entry.Skip)
                {
                    Screen.Write(entry.ToString());
                    if (Screen.Row >= Screen.Rows - 2)
                        break;
                    Screen.Row++;
                    Screen.Column = 0;
                }

                depth++;
            }
        }
    }
}
