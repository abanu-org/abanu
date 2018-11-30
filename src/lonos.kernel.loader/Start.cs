using System;
using Mosa.Runtime;
using Mosa.Kernel.x86;
using Mosa.Runtime.Plug;
using Mosa.Runtime.x86;

namespace lonos.kernel.core
{

    internal static class Start
    {

        public unsafe static void Main()
        {
            Native.Nop();
            Native.Nop();
            jump();

            Native.Nop();
            Native.Nop();

            Serial.SetupPort(Serial.COM1);
            KernelMessage.WriteLine("hello");


            // Detect environment (Memory Maps, Video Mode, etc.)
            Multiboot.Setup();


            // Setup Global Descriptor Table
            //GDT.Setup();

            //PageTable.Setup();
            Debug.Break();
        }

        private const uint addr = 0x04100000+0x1000 + 0x30;

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private static void jump() {
            Native.Jmp(addr);
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

    }
}
