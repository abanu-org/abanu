// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System;
using Abanu.Kernel.Core.Processes;
using Abanu.Kernel.Core.Scheduling;

namespace Abanu.Kernel.Core.Interrupts
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
