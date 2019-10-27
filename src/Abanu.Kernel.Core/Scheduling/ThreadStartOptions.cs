// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System.Threading;
using Abanu.Kernel.Core.MemoryManagement;
using Mosa.Runtime;

namespace Abanu.Kernel.Core.Scheduling
{

    public struct ThreadStartOptions
    {
        public Addr MethodAddr;
        public uint StackSize;
        public bool AllowUserModeIOPort;
        public bool Debug;
        public string DebugName;
        public uint ArgumentBufferSize;
        public int Priority;

        public ThreadStartOptions(ThreadStart start)
        {
            MethodAddr = (uint)Intrinsic.GetDelegateMethodAddress(start);
            Memory.FreeObject(start);
            AllowUserModeIOPort = KConfig.AllowUserModeIOPort;
            StackSize = KConfig.DefaultStackSize;
            Debug = false;
            DebugName = null;
            ArgumentBufferSize = 0;
            Priority = 0;
        }

        public ThreadStartOptions(Addr methodAddr)
        {
            MethodAddr = methodAddr;
            AllowUserModeIOPort = KConfig.AllowUserModeIOPort;
            StackSize = KConfig.DefaultStackSize;
            Debug = false;
            DebugName = null;
            ArgumentBufferSize = 0;
            Priority = 0;
        }

    }

}
