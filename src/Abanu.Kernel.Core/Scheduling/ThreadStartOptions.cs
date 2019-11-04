// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System.Threading;
using Abanu.Kernel.Core.MemoryManagement;
using Mosa.Runtime;

namespace Abanu.Kernel.Core.Scheduling
{

    public struct ThreadStartOptions
    {

        /// <summary>
        /// Entry point of the Thread
        /// </summary>
        public Addr MethodAddr;

        /// <summary>
        /// Initial Stack size
        /// </summary>
        public uint StackSize;

        /// <summary>
        /// If true, the thread can access IO ports, when running in user mode.
        /// </summary>
        public bool AllowUserModeIOPort;

        /// <summary>
        /// If true, the Thread will be debugged.
        /// </summary>
        public bool Debug;
        public string DebugName;

        /// <summary>
        /// Required bytes for entrypoint arguments. Used to build the initial stack correctly.
        /// </summary>
        public uint ArgumentBufferSize;

        /// <summary>
        /// Initial Thread priority.
        /// </summary>
        /// <seealso cref="Thread.Priority"/>
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
