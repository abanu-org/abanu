using lonos.Kernel;
using lonos.Kernel.Core;
using System;

namespace lonos.Runtime
{

    /// <summary>
    /// Pure calls. This is no Framwork. No helpers!
    /// </summary>
    public static class SysCalls
    {
        public static uint RequestMemory(uint size)
        {
            return MessageManager.Send(SysCallTarget.RequestMemory, size);
        }

        // TODO: Datetime
        public static long GetSystemTime()
        {
            throw new NotImplementedException();
        }
    }

}
