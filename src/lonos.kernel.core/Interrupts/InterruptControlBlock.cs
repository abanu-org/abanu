using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace lonos.Kernel.Core.Interrupts
{
    [StructLayout(LayoutKind.Sequential)]
    public struct InterruptControlBlock
    {
        public uint KernelPageTableAddr;
    }
}
