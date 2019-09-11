// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lonos.Kernel.Core
{
    public enum X86_EFlags : uint
    {
        None = 0,
        CarryFlag = BitMask.Bit0,
        Reserved1 = BitMask.Bit1,
        ParityFlag = BitMask.Bit2,
        AuxiliaryCarryFlag = BitMask.Bit3,
        Reserved4 = BitMask.Bit4,
        Reserved5 = BitMask.Bit5,
        ZeroFlag = BitMask.Bit6,
        SignFlag = BitMask.Bit7,
        TrapFlag = BitMask.Bit8,
        InterruptEnableFlag = BitMask.Bit9,
        DirectionFlag = BitMask.Bit10,
        OverflowFlag = BitMask.Bit11,
        IOPrivilegeLevel = BitMask.Bit12 | BitMask.Bit13,
        NestedTask = BitMask.Bit14,
        Reserved15 = BitMask.Bit15,
        ResumeFlag = BitMask.Bit16,
        Virtual8086Mode = BitMask.Bit17,
        AlignmentCheck = BitMask.Bit18,
        VirtualInterruptFlag = BitMask.Bit19,
        VirtualInterruptPending = BitMask.Bit20,
        IDFlag = BitMask.Bit21,
    }
}
