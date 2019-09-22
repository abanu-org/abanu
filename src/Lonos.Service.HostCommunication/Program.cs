﻿// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Lonos.Kernel.Core;
using Lonos.Runtime;
using Mosa.Runtime.x86;

namespace Lonos.Kernel
{

    public static class Program
    {

        public static unsafe void Main()
        {
            ApplicationRuntime.Init();

            MessageManager.OnMessageReceived = MessageReceived;

            var port = Serial.COM2;
            Serial.SetupPort(port);

            var path = "test";
            var fileSize = (uint)GetFileLenth(path);
            var target = SysCalls.GetProcessIDForCommand(SysCallTarget.CreateMemoryProcess);
            var fileBuf = SysCalls.RequestMessageBuffer((uint)fileSize, target);
            var handle = OpenFile(path);
            var bufSize = 3000u;
            var buf = (byte*)RuntimeMemory.Allocate(bufSize);
            var gotBytes = (uint)ReadFile(handle, buf, bufSize);
            var fileBufPos = 0u;
            while (gotBytes > 0)
            {
                //Console.WriteLine("got data");
                for (var i = 0; i < gotBytes; i++)
                    ((byte*)fileBuf.Start)[fileBufPos + i] = buf[i];
                fileBufPos += gotBytes;
                gotBytes = (uint)ReadFile(handle, buf, bufSize);
            }
            RuntimeMemory.Free(buf);
            SysCalls.CreateMemoryProcess(fileBuf, fileSize);
            //RuntimeMemory.Free(fileBuf);

            while (true)
            {
            }
        }

        public static unsafe void MessageReceived(SystemMessage* msg)
        {
            MessageManager.Send(new SystemMessage(SysCallTarget.ServiceReturn, msg->Arg1 + 10));
        }

        public unsafe struct MessageHeader
        {
            public int MsgId;
            public MessageCommand Command;
        }

        public enum MessageCommand
        {
            OpenFile = 240,
            WriteFile = 241,
            ReadFile = 242,
            GetFileLength = 243,
        }

        private static int lastMessageId = 0;
        public static unsafe FileHandle OpenFile(string path)
        {
            var msgId = ++lastMessageId;
            WriteHeader(new MessageHeader { Command = MessageCommand.OpenFile, MsgId = msgId });
            WriteArg(path);
            WriteEnd();
            return ReadResultInt32(msgId);
        }

        public static unsafe int GetFileLenth(string path)
        {
            var msgId = ++lastMessageId;
            WriteHeader(new MessageHeader { Command = MessageCommand.GetFileLength, MsgId = msgId });
            WriteArg(path);
            WriteEnd();
            return ReadResultInt32(msgId);
        }

        private static int ReadResultInt32(int msgId)
        {
            var lineType = ReadByte();
            Assert.True(lineType == (byte)LineType.Result);

            var msgId_ = ReadInt32();
            Assert.True(msgId_ == msgId);

            return ReadInt32();
        }

        public static unsafe int ReadFile(FileHandle handle, byte* buf, uint bufSize)
        {
            return -1;
        }

        public static unsafe int WriteFile(FileHandle handle, byte* buf, uint bufSize)
        {
            return -1;
        }

        public enum LineType : byte
        {
            Header = 200,
            Arg = 201,
            Data = 202,
            End = 203,
            Result = 204,
        }

        public static unsafe void WriteHeader(MessageHeader header)
        {
            var size = (uint)sizeof(MessageHeader);
            Write((byte)LineType.Header);
            var data = (byte*)(&header);
            for (var i = 0; i < size; i++)
                Write(data[i]);
        }

        public static unsafe void WriteData(uint bufSize, byte* data)
        {
            WriteDataStart(bufSize);
            for (var i = 0; i < bufSize; i++)
                WriteData(data[i]);
        }

        public static unsafe void WriteData(string data)
        {
            WriteDataStart((uint)data.Length);
            for (var i = 0; i < data.Length; i++)
                Write((byte)data[i]);
        }

        public static unsafe void WriteArg(string data)
        {
            WriteArgStart((uint)data.Length);
            for (var i = 0; i < data.Length; i++)
                Write((byte)data[i]);
        }

        public static unsafe void WriteEnd()
        {
            Write((byte)LineType.End);
        }

        private static unsafe void WriteDataStart(uint dataLength)
        {
            Write((byte)LineType.Data);
            Write(dataLength);
        }

        private static unsafe void WriteArgStart(uint argSize)
        {
            Write((byte)LineType.Arg);
            Write(argSize);
        }

        public static unsafe void WriteData(byte data)
        {
            Write(data);
        }

        private static unsafe void Write(uint data)
        {
            var bytes = BitConversion.GetBytes(data);
            Write(bytes);
            RuntimeMemory.FreeObject(bytes);
        }

        private static unsafe void Write(byte[] data)
        {
            for (var i = 0; i < data.Length; i++)
                Write(data[i]);
        }

        private static unsafe void Write(string data)
        {
            for (var i = 0; i < data.Length; i++)
                Write((byte)data[i]);
        }

        private static unsafe void Write(byte data)
        {
            Serial.Write(Serial.COM2, data);
        }

        private static byte ReadByte()
        {
            return Serial.Read(Serial.COM2);
        }

        private static int ReadInt32()
        {
            var buf = new byte[4];
            buf[0] = ReadByte();
            buf[1] = ReadByte();
            buf[2] = ReadByte();
            buf[3] = ReadByte();
            var result = BitConversion.GetInt32(buf);
            RuntimeMemory.FreeObject(buf);
            return result;
        }

        private static uint ReadUInt32()
        {
            return (uint)ReadInt32();
        }

    }
}