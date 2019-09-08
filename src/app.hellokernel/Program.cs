using lonos.Kernel.Core;
using lonos.Runtime;
using Mosa.Runtime.x86;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace lonos.Kernel
{

    public static class Program
    {

        public static void Main()
        {
            ApplicationRuntime.Init();

            //var result = MessageManager.Send(SysCallTarget.ServiceFunc1, 55);

            var writeDebugMessageProcID = SysCalls.GetProcessIDForCommand(SysCallTarget.WriteDebugMessage);
            var buf = SysCalls.RequestMessageBuffer(4096, writeDebugMessageProcID);
            SysCalls.WriteDebugMessage(buf, "Hello Kernel!");

            while (true) { }
        }

    }
}
