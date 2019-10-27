// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Abanu.Kernel;

namespace Abanu.Runtime
{

    public unsafe delegate void OnMessageReceivedDelegate(SystemMessage* msg);
    public delegate void OnExceptionDelegate(Exception ex);
    //public unsafe delegate void OnInterruptReceivedDelegate(InterruptMessage* msg);

    public static class MessageManager
    {

        public static OnMessageReceivedDelegate OnMessageReceived;
        public static OnExceptionDelegate OnDispatchError;
        //public static OnInterruptReceivedDelegate OnInterruptReceived;

        public static unsafe void Dispatch(SystemMessage msg)
        {
            try
            {
                if (OnMessageReceived != null)
                    OnMessageReceived(&msg);
            }
            catch (Exception ex)
            {
                if (OnDispatchError != null)
                    OnDispatchError(ex);
            }
        }

        //public static unsafe void DispatchInterrupt(InterruptMessage msg)
        //{
        //    if (OnInterruptReceived != null)
        //        OnInterruptReceived(&msg);
        //}

        [DllImport("x86/Abanu.SysCall.o", EntryPoint = "SysCallInt")]
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
