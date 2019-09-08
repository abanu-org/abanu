using lonos.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace lonos.Runtime
{

    public unsafe delegate void OnMessageReceivedDelegate(SystemMessage* msg);

    public static class MessageManager
    {

        public static OnMessageReceivedDelegate OnMessageReceived;

        public unsafe static void Dispatch(SystemMessage msg)
        {
            if (OnMessageReceived != null)
                OnMessageReceived(&msg);
        }

        [DllImport("x86/app.HelloKernel.o", EntryPoint = "SysCallInt")]
        private extern static uint SysCallInt(SystemMessage msg);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public unsafe static uint Send(SystemMessage msg)
        {
            return SysCallInt(msg);
        }

        public unsafe static uint Send(SysCallTarget target, uint arg1 = 0, uint arg2 = 0, uint arg3 = 0, uint arg4 = 0, uint arg5 = 0, uint arg6 = 0)
        {
            return SysCallInt(new SystemMessage(target, arg1, arg2, arg3, arg4, arg5, arg6));
        }

        public unsafe static uint Send(uint target, uint arg1 = 0, uint arg2 = 0, uint arg3 = 0, uint arg4 = 0, uint arg5 = 0, uint arg6 = 0)
        {
            return SysCallInt(new SystemMessage(target, arg1, arg2, arg3, arg4, arg5, arg6));
        }

    }

}
