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

    }

}
