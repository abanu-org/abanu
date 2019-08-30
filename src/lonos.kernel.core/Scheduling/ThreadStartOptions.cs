// Adopted Implementation from: MOSA Project

using lonos.Kernel.Core.MemoryManagement;
using Mosa.Runtime;
using System.Threading;

namespace lonos.Kernel.Core.Scheduling
{

    public struct ThreadStartOptions
    {
        public Addr MethodAddr;
        public uint StackSize;
        public bool AllowUserModeIOPort;

        public ThreadStartOptions(ThreadStart start)
        {
            MethodAddr = Intrinsic.GetDelegateMethodAddress(start);
            Memory.FreeObject(start);
            AllowUserModeIOPort = KConfig.AllowUserModeIOPort;
            StackSize = KConfig.DefaultStackSize;
        }

        public ThreadStartOptions(Addr methodAddr)
        {
            MethodAddr = methodAddr;
            AllowUserModeIOPort = KConfig.AllowUserModeIOPort;
            StackSize = KConfig.DefaultStackSize;
        }

    }

}
