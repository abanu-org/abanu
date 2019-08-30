using lonos.Kernel.Core.Elf;
using lonos.Kernel.Core.PageManagement;
using lonos.Kernel.Core.Scheduling;
using Mosa.Runtime.x86;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace lonos.Kernel.Core.Processes
{

    public static class ProcessManager
    {

        private static uint NextCreateProcessID;
        public static Process Idle;
        public static Process System;

        public static void Setup(ThreadStart followupTask)
        {
            Idle = CreateEmptyProcess(new ProcessCreateOptions());
            Idle.Path = "/system/idle";

            System = CreateEmptyProcess(new ProcessCreateOptions());
            System.Path = "/system/main";

            Scheduler.Setup(followupTask);
            Scheduler.Start();

            while (true)
                Native.Hlt();
        }

        public static Process CreateEmptyProcess(ProcessCreateOptions options)
        {
            var proc = new Process();
            proc.ProcessID = NextCreateProcessID++; // TODO: Interlocked
            proc.User = options.User;
            proc.PageTable = PageTable.KernelTable;
            return proc;
        }

        public static void StartProcess(string path)
        {
            var elf = KernelElf.FromSectionName(path);
            for (var i = 0; i < elf.SectionHeaderCount; i++)
            {

            }
        }
    }

}
