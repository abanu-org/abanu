using lonos.Kernel;
using lonos.Kernel.Core;

namespace lonos.Runtime
{
    public static class SysCalls
    {
        public static uint RequestMemory(uint size)
        {
            return MessageManager.Send(new SystemMessage { Command = 20, Arg1 = size });
        }
    }

}
