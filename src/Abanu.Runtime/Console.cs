// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using Abanu.Kernel;
using Abanu.Kernel.Core;
using Abanu.Runtime;

namespace Abanu.Runtime
{
    public static class Console
    {

        private static MemoryRegion buf;

        static Console()
        {
            var writeDebugMessageProcID = SysCalls.GetProcessIDForCommand(SysCallTarget.WriteDebugMessage);
            buf = SysCalls.RequestMessageBuffer(4096, writeDebugMessageProcID);
        }

        public static void Write(string msg)
        {
            SysCalls.WriteDebugMessage(buf, msg);
        }

        public static void WriteLine(string msg)
        {
            SysCalls.WriteDebugMessage(buf, msg);
            SysCalls.WriteDebugChar('\n');
        }

        public static void WriteLine()
        {
            SysCalls.WriteDebugChar('\n');
        }

        public static void Write(char c)
        {
            SysCalls.WriteDebugChar(c);
        }
    }

}
