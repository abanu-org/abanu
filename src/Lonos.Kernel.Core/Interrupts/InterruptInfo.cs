// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using Lonos.Kernel.Core.Processes;
using Lonos.Kernel.Core.Scheduling;

namespace Lonos.Kernel.Core.Interrupts
{
    public struct InterruptInfo
    {
        public InterruptHandler PreHandler;
        public InterruptHandler Handler;
        public bool CountStatistcs;
        public bool Trace;
        public int Interrupt;
        public bool NotifyUnhandled;
        public Service Service;
    }
}
