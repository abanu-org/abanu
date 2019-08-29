using System;
namespace lonos.kernel.core.Interrupts
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
        Keyboard = 33
    }

}
