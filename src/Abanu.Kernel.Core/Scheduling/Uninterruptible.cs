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

    /// <summary>
    /// Ensure a non-interruptible code path
    /// </summary>
    public static class Uninterruptible
    {

        /// <summary>
        /// Disables Interrupts
        /// </summary>
        /// <returns>Returns true, if interrupts where successful disabled. Return false, if interrupts was already disabled.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Enter()
        {

            // This check is correct.
            // If Interrupts are disabled: Nobody can change it's state. We have exclusive control.
            // If Interrupts are enabled: This doesn't matter, because the other routine will re-enable it on ISR exit.
            // So, InterrupsEnabled() is a check, if the current thread is an ISR or not.

            if (IDTManager.InterrupsEnabled())
            {
                Native.Cli();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Re-Enables Interrupts. Assuming prior <see cref="Enter"/> call returned true.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Exit()
        {
            Exit(true);
        }

        /// <summary>
        /// Re-Enables Interrupts. Pass <see cref="Enter"/> from prior <see cref="Enter"/>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Exit(bool enterStatus)
        {
            if (enterStatus)
                Native.Sti();
        }

        /// <summary>
        /// Executes a non-interruptible code-path.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Execute(Action callback)
        {
            if (callback == null)
                return;

            if (Enter())
            {
                try
                {
                    callback();
                }
                finally
                {
                    Exit();
                }
            }
            else
            {
                callback();
            }
        }

    }
}
