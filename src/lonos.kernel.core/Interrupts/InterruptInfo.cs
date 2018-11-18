using System;

namespace lonos.kernel.core
{
    public struct InterruptInfo
    {
        public InterruptHandler Handler;
        public bool CountStatistcs;
        public bool Trace;
        public int Interrupt;
    }
}
