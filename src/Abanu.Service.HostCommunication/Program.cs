// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Abanu.Kernel.Core;
using Abanu.Runtime;
using Mosa.Runtime.x86;

namespace Abanu.Kernel
{

    public static class Program
    {

        public static unsafe void Main()
        {
            ApplicationRuntime.Init();

            MessageManager.OnMessageReceived = MessageReceived;

            HostCommunicator.Init();

            //RuntimeMemory.Free(fileBuf);
            SysCalls.WriteDebugChar('+');
            while (true)
            {
                SysCalls.ThreadSleep(0);
            }
        }

        public static unsafe void MessageReceived(in SystemMessage msg)
        {
            MessageManager.Send(new SystemMessage(SysCallTarget.ServiceReturn, msg.Arg1 + 10));
        }

    }
}
