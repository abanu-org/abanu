namespace lonos.Kernel
{
    public enum SysCallTarget
    {
        RequestMemory = 20,
        ServiceReturn = 21,
        ServiceFunc1 = 22,
        RequestMessageBuffer = 23,
        WriteDebugMessage = 24,
        GetProcessIDForCommand = 25,
        GetPhysicalMemory = 26,
        TranslateVirtualToPhysicalAddress = 27,
        WriteDebugChar = 28,
    }

}
