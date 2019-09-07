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

        public unsafe static void Main()
        {
            ApplicationRuntime.Init();

            MessageManager.OnMessageReceived = MessageReceived;

            while (true) { }
        }

        public unsafe static void MessageReceived()
        {
            //MessageManager.Send(new SystemMessage { Command = 21, Arg2 = args->Arg1 + 10 });
        }

    }
}
