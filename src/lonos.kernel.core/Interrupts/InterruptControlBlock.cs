// Copyright (c) Lonos Project. All rights reserved.
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace lonos.Kernel.Core.Interrupts
{
    [StructLayout(LayoutKind.Sequential)]
    public struct InterruptControlBlock
    {
        public uint KernelPageTableAddr;
    }
}
