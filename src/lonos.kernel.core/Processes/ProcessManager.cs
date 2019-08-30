using lonos.Kernel.Core.Elf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lonos.Kernel.Core.Processes
{

    public static class ProcessManager
    {
        public static void StartProcess(string path)
        {
            var elf = KernelElf.FromSectionName(path);
            for (var i = 0; i < elf.SectionHeaderCount; i++)
            {

            }
        }
    }

}
