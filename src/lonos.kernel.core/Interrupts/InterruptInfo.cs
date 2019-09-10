// Copyright (c) Lonos Project. All rights reserved.
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;

namespace lonos.Kernel.Core.Interrupts
{
    public struct InterruptInfo
    {
        public InterruptHandler Handler;
        public bool CountStatistcs;
        public bool Trace;
        public int Interrupt;
    }
}
