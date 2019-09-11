// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Lonos.Kernel;

namespace Lonos.Runtime
{

    public unsafe delegate void OnMessageReceivedDelegate(SystemMessage* msg);

    public static class MessageManager
    {

        public static OnMessageReceivedDelegate OnMessageReceived;

        public static unsafe void Dispatch(SystemMessage msg)
        {
            if (OnMessageReceived != null)
                OnMessageReceived(&msg);
        }

        [DllImport("x86/app.HelloKernel.o", EntryPoint = "SysCallInt")]
        private static extern uint SysCallInt(SystemMessage msg);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static unsafe uint Send(SystemMessage msg)
        {
            return SysCallInt(msg);
        }

        public static unsafe uint Send(SysCallTarget target, uint arg1 = 0, uint arg2 = 0, uint arg3 = 0, uint arg4 = 0, uint arg5 = 0, uint arg6 = 0)
        {
            return SysCallInt(new SystemMessage(target, arg1, arg2, arg3, arg4, arg5, arg6));
        }

        public static unsafe uint Send(uint target, uint arg1 = 0, uint arg2 = 0, uint arg3 = 0, uint arg4 = 0, uint arg5 = 0, uint arg6 = 0)
        {
            return SysCallInt(new SystemMessage(target, arg1, arg2, arg3, arg4, arg5, arg6));
        }

    }

}
