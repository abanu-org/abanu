// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Abanu.Kernel.Core.Interrupts;
using Mosa.Runtime;
using Mosa.Runtime.x86;

namespace Abanu.Kernel.Core.Scheduling
{
    public static class UninterruptibleMonitor
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Enter(object obj)
        {
            var sync = (int)Intrinsic.GetObjectAddress(obj) + IntPtr.Size;

            if (Uninterruptible.Enter())
            {
                while (Native.CmpXChgLoad32(sync, 2, 0) != 0)
                {
                }
            }
            else
            {
                while (Native.CmpXChgLoad32(sync, 1, 0) != 0)
                {
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Exit(object obj)
        {
            var sync = (uint)(Intrinsic.GetObjectAddress(obj) + IntPtr.Size);

            var value = Native.Get32(sync);
            Native.Set32(sync, 0);

            Uninterruptible.Exit(value == 2);
        }

    }
}
