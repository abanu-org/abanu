// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using Lonos.Kernel;
using Lonos.Kernel.Core;

namespace Lonos.Runtime
{

    /// <summary>
    /// Pure calls. This is no Framwork. No helpers!
    /// </summary>
    public static unsafe class SysCalls
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

        public static void SetThreadPriority(int priority)
        {
            MessageManager.Send(SysCallTarget.SetThreadPriority, (uint)priority);
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

        public static FileHandle OpenFile(MemoryRegion buf, string path)
        {
            var data = (char*)buf.Start;
            for (var i = 0; i < path.Length; i++)
                data[i] = path[i];
            return (int)MessageManager.Send(SysCallTarget.OpenFile, buf.Start, (uint)path.Length);
        }

        public static SSize ReadFile(FileHandle handle, MemoryRegion buf)
        {
            return MessageManager.Send(SysCallTarget.ReadFile, buf.Start, buf.Size);
        }

        public static SSize WriteFile(FileHandle handle, MemoryRegion buf)
        {
            return MessageManager.Send(SysCallTarget.WriteFile, buf.Start, buf.Size);
        }

        public static SSize CreateFifo(MemoryRegion buf, string path)
        {
            var data = (char*)buf.Start;
            for (var i = 0; i < path.Length; i++)
                data[i] = path[i];
            return (int)MessageManager.Send(SysCallTarget.CreateFifo, buf.Start, (uint)path.Length);
        }

        public static void CreateMemoryProcess(MemoryRegion buf, uint size)
        {
            MessageManager.Send(SysCallTarget.CreateMemoryProcess, buf.Start, size);
        }

        public static void WriteDebugChar(char c)
        {
            MessageManager.Send(SysCallTarget.WriteDebugChar, (byte)c);
        }

    }

}
