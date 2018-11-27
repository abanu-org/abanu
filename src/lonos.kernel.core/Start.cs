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
            ApiContext.Current = new ApiHost();

            // Setup some pseudo devices
            Devices.InitStage1();

            //Setup Output and Debug devices
            Devices.InitStage2();

            // Write first output
            KernelMessage.WriteLine("<CONSOLE:BEGIN>");
            KernelMessage.WriteLine("Booting Lonos Kernel...");

            // Detect environment (Memory Maps, Video Mode, etc.)
            Multiboot.Setup();

            // Read own ELF-Headers and Sections
            KernelElf.Setup();

            // Initialize the embedded code (actually only a little proof of conecept code)
            NativeCalls.Setup();

            // Setup Global Descriptor Table
            GDT.Setup();

            // Now we enable Paging. It's important that we do not cause a Page Fault Exception,
            // Because IDT is not setup yet, that could handle this kind of exception.

            PageFrameAllocator.Setup();
            PageTable.Setup();
            PageFrameAllocator.Setup();
            Memory.Setup();

            Devices.InitFrameBuffer();

            // Now we are in virtual Adress Space !
            // Not requied yet, but maybe some re-initialization of should be done now.

            // Setup Programmable Interrupt Table
            PIC.Setup();

            // Setup Interrupt Descriptor Table
            // Important Note: IDT depends on GDT. Never setup IDT before GDT.
            IDTManager.Setup();

            var ptr = Memory.Allocate(1024, GFP.GFP_KERNEL);
            KernelMessage.WriteLine(ptr);
            var sb = new StringBuffer("ABCDEFGHIJKLMNOPQRSTU");
            sb.WriteTo(ptr);

            // We have nothing todo (yet). So let's stop.
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
