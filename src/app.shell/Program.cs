using Lonos.Kernel.Core;
using Lonos.Runtime;
using Mosa.Runtime.x86;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lonos.Kernel
{

    public static class Program
    {

        public static void Main()
        {
            ApplicationRuntime.Init();

            var result = MessageManager.Send(SysCallTarget.ServiceFunc1, 55);

            while (true) { }
        }

    }
}
