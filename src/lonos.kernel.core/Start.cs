using System;
using Mosa.Runtime;
using Mosa.Kernel.x86;
using Mosa.Runtime.Plug;
using Mosa.Runtime.x86;

namespace lonos.kernel.core
{

    internal static class Start
    {

        public static uint testValue = 0xAABB;

        public unsafe static void Main()
        {
            Debug.Setup();
            Debug.WriteLine("Booting...");

            Multiboot.Setup();

            KernelElf.Setup();
            NativeCalls.Setup();

            NativeCalls.proc2();
            NativeCalls.proc1();

            RawWrite(0, 6, '6', 1);
            GDT.Setup();

            RawWrite(0, 1, '1', 1);
            IDT.SetInterruptHandler(null);
            RawWrite(0, 2, '2', 1);
            Panic.Setup();
            RawWrite(0, 3, '3', 1);
            PIC.Setup();
            RawWrite(0, 4, '4', 1);
            IDT.Setup();
            RawWrite(0, 5, '5', 1);

            //Panic.DumpMemory(Address.GDTTable);

            //Memory.Init();


            RawWrite(0, 10, 'A', 1);
            USize size = 5;
            RawWrite(0, size, 'B', 1);
            RawWrite(0, 11, 'C', 1);

            Debug.Break();
        }

        private static void Dummy()
        {
            //This is a dummy call, that get never executed.
            //Its requied, because we need a real reference to Mosa.Runtime.x86
            //Without that, the .NET compiler will optimize that reference away
            //if its nowhere used. Than the Compiler dosnt know about that Refernce
            //and the Compilation will fail
            Mosa.Runtime.x86.Internal.GetStackFrame(0);
        }

        public const uint Columns = 80;

        /// <summary>
        /// The rows
        /// </summary>
        public const uint Rows = 40;

        public static void RawWrite(uint row, uint column, char chr, byte color)
        {
            IntPtr address = new IntPtr(0x0B8000 + ((row * Columns + column) * 2));

            Intrinsic.Store8(address, (byte)chr);
            Intrinsic.Store8(address, 1, color);
        }

    }
}
