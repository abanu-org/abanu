// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System;
using Abanu.Kernel.Core.Processes;
using Abanu.Kernel.Core.Scheduling;

namespace Abanu.Kernel.Core.Interrupts
{
    /// <summary>
    /// Holds informations for a specific IRQ
    /// </summary>
    public struct InterruptInfo
    {

        /// <summary>
        /// Always called before <see cref="Handler"/>
        /// </summary>
        public InterruptHandler PreHandler;

        /// <summary>
        /// The primary handler
        /// </summary>
        public InterruptHandler Handler;

        /// <summary>
        /// Raised statistics
        /// </summary>
        public bool CountStatistcs;

        /// <summary>
        /// Determines, if this Handler should debugged, if <see cref="KConfig.Log.Interrupts"/> is enabled
        /// </summary>
        public bool Trace;

        /// <summary>
        /// The Interrupt number / IRQ
        /// </summary>
        public int Interrupt;

        /// <summary>
        /// Generates a notice, if this handler is unhandled
        /// </summary>
        public bool NotifyUnhandled;

        /// <summary>
        /// Holds a reference to the dispatching service for this handler
        /// </summary>
        public Service Service;
    }
}
