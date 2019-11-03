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
    public static class Uninterruptable
    {

        /// <summary>
        /// Disables Interrupts
        /// </summary>
        /// <returns>returns true, if interrupts where disabled. Return false, if interrupts was already disabled.</returns>
        public static bool Enter()
        {
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
        public static void Exit()
        {
            Exit(true);
        }

        /// <summary>
        /// Re-Enables Interrupts. Pass <see cref="Enter"/> from prior <see cref="Enter"/>
        /// </summary>
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
