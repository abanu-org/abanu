// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using Abanu.Kernel;
using Abanu.Runtime;

namespace System.IO
{
    public static class File
    {

        public static Stream Open(string path)
        {
            var targetProcessId = SysCalls.GetProcessIDForCommand(SysCallTarget.OpenFile);
            var buf = SysCalls.RequestMessageBuffer(4096, targetProcessId);
            var handle = SysCalls.OpenFile(buf, path);
            return new FileStream(handle);
        }

    }

}
