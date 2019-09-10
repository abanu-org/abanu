// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;

namespace lonos.Kernel.Core.Interrupts
{

    public enum KnownInterrupt : uint
    {

        DivideError = 0,
        ArithmeticOverflowException = 4,
        BoundCheckError = 5,
        InvalidOpcode = 6,
        CoProcessorNotAvailable = 7,
        DoubleFault = 8,
        CoProcessorSegmentOverrun = 9,
        InvalidTSS = 10,
        SegmentNotPresent = 11,
        StackException = 12,
        GeneralProtectionException = 13,
        PageFault = 14,
        CoProcessorError = 16,
        SIMDFloatinPointException = 19,
        ClockTimer = 32,
        Keyboard = 33,

        // Custom Interrupts
        TerminateCurrentThread = 254,
    }

}
