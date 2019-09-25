// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

namespace Lonos.Kernel
{

    public enum SysCallTarget
    {
        Unset = 0,

        ServiceReturn = 1,
        Interrupt = 2,

        GetProcessIDForCommand = 10,

        RequestMessageBuffer = 20,
        RequestMemory = 21,
        GetPhysicalMemory = 22,
        TranslateVirtualToPhysicalAddress = 23,

        OpenFile = 30,
        ReadFile = 31,
        WriteFile = 32,
        CreateFifo = 33,

        CreateMemoryProcess = 40,
        SetThreadPriority = 41,

        SetServiceStatus = 50,
        RegisterService = 51,
        RegisterInterrupt = 52,

        WriteDebugMessage = 60,
        WriteDebugChar = 61,

        //ServiceFunc1 = 100,

        HostCommunication_OpenFile = 110,
        HostCommunication_ReadFile = 111,
        HostCommunication_WriteFile = 112,
        HostCommunication_CreateProcess = 113,

    }

}
