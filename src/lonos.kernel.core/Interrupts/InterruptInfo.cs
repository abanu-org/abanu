using System;

namespace lonos.Kernel.Core.Interrupts
{
    public struct InterruptInfo
    {
        public InterruptHandler Handler;
        public bool CountStatistcs;
        public bool Trace;
        public int Interrupt;
    }
}
