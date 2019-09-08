using lonos.Kernel;
using lonos.Kernel.Core;
using System;

namespace lonos.Runtime
{

    /// <summary>
    /// Pure calls. This is no Framwork. No helpers!
    /// </summary>
    public unsafe static class SysCalls
    {
        public static uint RequestMemory(uint size)
        {
            return MessageManager.Send(SysCallTarget.RequestMemory, size);
        }

        public static uint GetProcessIDForCommand(SysCallTarget target)
        {
            return MessageManager.Send(SysCallTarget.GetProcessIDForCommand);
        }

        public static uint GetPhysicalMemory(Addr physAddr, USize size)
        {
            return MessageManager.Send(SysCallTarget.GetPhysicalMemory, physAddr, size);
        }

        public static uint TranslateVirtualToPhysicalAddress(Addr virtAddr)
        {
            return MessageManager.Send(SysCallTarget.TranslateVirtualToPhysicalAddress, virtAddr);
        }

        public static MemoryRegion RequestMessageBuffer(uint size, uint targetProcessID)
        {
            return new MemoryRegion(MessageManager.Send(SysCallTarget.RequestMessageBuffer, size, targetProcessID), size);
        }

        // TODO: Datetime
        public static long GetSystemTime()
        {
            throw new NotImplementedException();
        }

        public static void WriteDebugMessage(MemoryRegion buf, string message)
        {
            var data = (char*)buf.Start;
            for (var i = 0; i < message.Length; i++)
                data[i] = message[i];
            MessageManager.Send(SysCallTarget.WriteDebugMessage, buf.Start, (uint)message.Length);
        }

        public static void WriteDebugChar(char c)
        {
            MessageManager.Send(SysCallTarget.WriteDebugChar, (byte)c);
        }

    }

}
