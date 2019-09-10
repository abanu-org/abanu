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

        public unsafe static void Main()
        {
            ApplicationRuntime.Init();

            MessageManager.OnMessageReceived = MessageReceived;

            while (true) { }
        }

        public unsafe static void MessageReceived(SystemMessage* msg)
        {
            MessageManager.Send(SysCallTarget.ServiceReturn, msg->Arg1 + 10);
        }

    }
}
