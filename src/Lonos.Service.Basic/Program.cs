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

            MessageManager.OnMessageReceived = MessageReceived;

            Files.Add(new VfsFile { Path = "/dev/keyboard", Buffer = new FifoFile() });
            Files.Add(new VfsFile { Path = "/dev/screen", Buffer = new FifoFile() });

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

        internal class FifoFile : IBuffer
        {

            private LinkedList<byte> List = new LinkedList<byte>();

            public unsafe SSize Read(byte* buf, USize count)
            {
                if (List.Count == 0)
                    return 0;

                var cnt = Math.Min(count, List.Count);
                for (var i = 0; i < cnt; i++)
                    List.RemoveFirst();

                return cnt;
            }

            public unsafe SSize Write(byte* buf, USize count)
            {
                for (var i = 0; i < count; i++)
                    List.AddLast(buf[i]);

                return (uint)count;
            }
        }

        internal class OpenFile
        {
            public FileHandle Handle;
            public string Path;
            public int ProcessId;
            public IBuffer Buffer;
        }

        private static List<OpenFile> OpenFiles = new List<OpenFile>();

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

        private static List<VfsFile> Files = new List<VfsFile>();

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
            var length = msg->Arg2;
            var data = (char*)start;

            var path = new string(data);

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
