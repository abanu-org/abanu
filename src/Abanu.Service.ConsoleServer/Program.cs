// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
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

            var targetProcessId = SysCalls.GetProcessIDForCommand(SysCallTarget.OpenFile);
            var buf = SysCalls.RequestMessageBuffer(4096, targetProcessId);
            var conHandle = SysCalls.OpenFile(buf, "/dev/console");

            var con = new ConsoleServer();
            con.Write("ConsoleServer Started");

            // TODO: Create dev /dev/console, other processes can check their existence
            SysCalls.SetServiceStatus(ServiceStatus.Ready);

            while (true)
            {
                SysCalls.ThreadSleep(0); // TODO: Signal

                var gotBytes = SysCalls.ReadFile(conHandle, buf);
                if (gotBytes > 0)
                {
                    for (var byteIdx = 0; byteIdx < gotBytes; byteIdx++)
                    {
                        var bufPtr = (byte*)buf.Start;
                        var key = bufPtr[byteIdx];
                        con.Write(key);
                    }
                }
            }
        }

        public static unsafe void OnDispatchError(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

    }

}
