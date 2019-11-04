// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abanu.Kernel.Core.MemoryManagement;
using Abanu.Kernel.Core.Processes;
using Abanu.Kernel.Core.Scheduling;

namespace Abanu.Kernel.Core.Diagnostics
{

    public static class KDebug
    {
        public static void DumpStats()
        {
            Scheduler.DumpStats();
            ProcessManager.DumpStats();
            VirtualPageManager.DumpStats();
        }
    }

}
