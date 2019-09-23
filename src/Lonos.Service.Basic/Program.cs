// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
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

            Files = new List<VfsFile>();
            OpenFiles = new List<OpenFile>();

            Files.Add(new VfsFile { Path = "/dev/keyboard", Buffer = new FifoFile() });
            Files.Add(new VfsFile { Path = "/dev/screen", Buffer = new FifoFile() });

            MessageManager.OnMessageReceived = MessageReceived;

            SysCalls.RegisterService(SysCallTarget.OpenFile);
            SysCalls.RegisterService(SysCallTarget.CreateFifo);
            SysCalls.RegisterService(SysCallTarget.ReadFile);
            SysCalls.RegisterService(SysCallTarget.WriteFile);

            SysCalls.SetServiceStatus(ServiceStatus.Ready);

            while (true)
            {
            }
        }

        public static unsafe void MessageReceived(SystemMessage* msg)
        {
            switch (msg->Target)
            {
                case SysCallTarget.OpenFile:
                    Cmd_OpenFile(msg);
                    break;
                case SysCallTarget.WriteFile:
                    Cmd_WriteFile(msg);
                    break;
                case SysCallTarget.ReadFile:
                    Cmd_ReadFile(msg);
                    break;
                case SysCallTarget.CreateFifo:
                    Cmd_CreateFiFo(msg);
                    break;
                default:
                    MessageManager.Send(new SystemMessage(SysCallTarget.ServiceReturn));
                    break;
            }
        }

        /// <summary>
        /// General purpose Fifo
        /// </summary>
        internal class FifoStream : IBuffer, IDisposable
        {
            private byte[] Data;
            private int WritePosition;
            private int ReadPosition;
            public int Length;

            public FifoStream(int capacity)
            {
                Data = new byte[capacity];
            }

            public unsafe SSize Read(byte* buf, USize count)
            {
                if (Length == 0)
                    return 0;

                var cnt = Math.Min(count, Length);
                for (var i = 0; i < cnt; i++)
                {
                    buf[i] = Data[ReadPosition++];
                    if (WritePosition >= Data.Length)
                        WritePosition = 0;
                    Length--;
                }

                return cnt;
            }

            public unsafe SSize Write(byte* buf, USize count)
            {
                for (var i = 0; i < count; i++)
                {
                    Data[WritePosition++] = buf[i];
                    if (WritePosition >= Data.Length)
                        WritePosition = 0;
                    Length++;
                }
                return (uint)count;
            }

            public void Dispose()
            {
                RuntimeMemory.FreeObject(Data);
            }
        }

        internal class FifoFile : IBuffer, IDisposable
        {
            private IBuffer Data;

            public FifoFile()
            {
                Data = new FifoStream(256);
            }

            public void Dispose()
            {
                RuntimeMemory.FreeObject(Data);
            }

            public unsafe SSize Read(byte* buf, USize count)
            {
                return Data.Read(buf, count);
            }

            public unsafe SSize Write(byte* buf, USize count)
            {
                return Data.Write(buf, count);
            }
        }

        internal class OpenFile
        {
            public FileHandle Handle;
            public string Path;
            public int ProcessId;
            public IBuffer Buffer;
        }

        private static List<OpenFile> OpenFiles;

        private static OpenFile FindOpenFile(FileHandle handle)
        {
            for (var i = 0; i < OpenFiles.Count; i++)
                if (OpenFiles[i].Handle == handle)
                    return OpenFiles[i];

            return null;
        }

        private static int lastHandle = 0x33776655;

        internal class VfsFile
        {
            public IBuffer Buffer;
            public string Path;
        }

        private static List<VfsFile> Files;

        internal static VfsFile FindFile(string path)
        {
            for (var i = 0; i < Files.Count; i++)
                if (Files[i].Path == path)
                    return Files[i];

            return null;
        }

        public static unsafe void Cmd_CreateFiFo(SystemMessage* msg)
        {
            var start = msg->Arg1;
            var length = (int)msg->Arg2;
            var data = (char*)start;

            var path = new string(data, 0, length);

            var fifo = new FifoFile()
            {
            };

            var vfsFile = new VfsFile
            {
                Path = path,
                Buffer = fifo,
            };
            Files.Add(vfsFile);

            MessageManager.Send(new SystemMessage(SysCallTarget.ServiceReturn));
        }

        public static unsafe void Cmd_CreateMemoryFile(SystemMessage* msg)
        {
            var start = msg->Arg1;
            var length = msg->Arg2;
            var data = (char*)start;

            var path = new string(data);

            var fifo = new FifoFile()
            {
            };

            var vfsFile = new VfsFile
            {
                Path = path,
                Buffer = fifo,
            };
            Files.Add(vfsFile);

            MessageManager.Send(new SystemMessage(SysCallTarget.ServiceReturn));
        }

        public static unsafe void Cmd_OpenFile(SystemMessage* msg)
        {
            var start = msg->Arg1;
            var length = (int)msg->Arg2;
            var data = (char*)start;

            var path = new string(data, 0, length);

            var file = FindFile(path);
            if (file == null)
            {
                MessageManager.Send(new SystemMessage(SysCallTarget.ServiceReturn, FileHandle.Zero));
                return;
            }

            var openFile = new OpenFile()
            {
                Handle = ++lastHandle,
                Path = path,
                ProcessId = -1,
                Buffer = file.Buffer,
            };
            OpenFiles.Add(openFile);

            MessageManager.Send(new SystemMessage(SysCallTarget.ServiceReturn, openFile.Handle));
        }

        public static unsafe void Cmd_ReadFile(SystemMessage* msg)
        {
            var openFile = FindOpenFile((int)msg->Arg1);
            var data = (byte*)msg->Arg2;
            var length = msg->Arg3;
            var gotBytes = openFile.Buffer.Read(data, length);
            MessageManager.Send(new SystemMessage(SysCallTarget.ServiceReturn, gotBytes));
        }

        public static unsafe void Cmd_WriteFile(SystemMessage* msg)
        {
            var openFile = FindOpenFile((int)msg->Arg1);
            var data = (byte*)msg->Arg2;
            var length = msg->Arg3;
            var gotBytes = openFile.Buffer.Write(data, length);
            MessageManager.Send(new SystemMessage(SysCallTarget.ServiceReturn, gotBytes));
        }

    }
}
