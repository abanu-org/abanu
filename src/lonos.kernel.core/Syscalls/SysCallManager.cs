using lonos.Kernel.Core.Interrupts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lonos.Kernel.Core.SysCalls
{
    public unsafe class SysCallManager
    {

        public const uint IRQ = 250;

        public static void Setup()
        {
            KernelMessage.WriteLine("Initialize SysCall Manager");

            IDTManager.SetInterruptHandler(IRQ, InterruptHandler);
            IDTManager.SetPrivilegeLevel(IRQ, 0x03);
            IDTManager.Flush();
        }

        public static void InterruptHandler(IDTStack* stack)
        {
            KernelMessage.WriteLine("got SysCall {0}", stack->Interrupt);
        }

    }

}
