// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lonos.Kernel.Core.Interrupts;
using Mosa.Runtime;
using Mosa.Runtime.x86;

namespace Lonos.Kernel.Core.Scheduling
{
    public static class UninterruptableMonitor
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Enter(object obj)
        {
            var sync = Intrinsic.GetObjectAddress(obj) + IntPtr.Size;

            if (IDTManager.InterrupsEnabled())
            {
                // Thread
                Native.Cli();

                while (Native.CmpXChgLoad32(sync.ToInt32(), 2, 0) != 0)
                {
                }

            }
            else
            {
                // ISR
                while (Native.CmpXChgLoad32(sync.ToInt32(), 1, 0) != 0)
                {
                }

            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Exit(object obj)
        {
            var sync = Intrinsic.GetObjectAddress(obj) + IntPtr.Size;

            var value = Native.Get32(sync.ToUInt32());
            Native.Set32(sync.ToUInt32(), 0);
            if (value == 2)
                Native.Sti();
        }

    }
}
