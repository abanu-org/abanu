using System;

namespace lonos.kernel.core.Interrupts
{
    public struct InterruptInfo
    {
        public InterruptHandler Handler;
        public bool CountStatistcs;
        public bool Trace;
        public int Interrupt;
    }
}
