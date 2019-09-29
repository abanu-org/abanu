// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System.Threading;
using Lonos.Kernel.Core.MemoryManagement;
using Mosa.Runtime;

namespace Lonos.Kernel.Core.Scheduling
{

    public struct ThreadStartOptions
    {
        public Addr MethodAddr;
        public uint StackSize;
        public bool AllowUserModeIOPort;
        public bool Debug;
        public string DebugName;
        public uint ArgumentBufferSize;

        public ThreadStartOptions(ThreadStart start)
        {
            MethodAddr = (uint)Intrinsic.GetDelegateMethodAddress(start);
            Memory.FreeObject(start);
            AllowUserModeIOPort = KConfig.AllowUserModeIOPort;
            StackSize = KConfig.DefaultStackSize;
            Debug = false;
            DebugName = null;
            ArgumentBufferSize = 0;
        }

        public ThreadStartOptions(Addr methodAddr)
        {
            MethodAddr = methodAddr;
            AllowUserModeIOPort = KConfig.AllowUserModeIOPort;
            StackSize = KConfig.DefaultStackSize;
            Debug = false;
            DebugName = null;
            ArgumentBufferSize = 0;
        }

    }

}
