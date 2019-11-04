// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Abanu.Kernel.Core.Collections;
using Abanu.Kernel.Core.Diagnostics;
using Abanu.Kernel.Core.Interrupts;
using Abanu.Kernel.Core.MemoryManagement;
using Abanu.Kernel.Core.PageManagement;
using Abanu.Kernel.Core.Processes;
using Abanu.Kernel.Core.Scheduling;
using static Abanu.Kernel.Core.Processes.Process;

namespace Abanu.Kernel.Core.SysCalls
{

    public class SysCallHandlerInfo
    {
        public SysCallTarget CommandID;
        public DSysCallInfoHandler Handler;

        /// <summary>
        /// The process that should handle this command
        /// </summary>
        public Process Process;

        /// <summary>
        /// if true, this command will be debugged
        /// </summary>
        public bool Debug;
    }

}
