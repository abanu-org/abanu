// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Abanu.Kernel.Core;
using Abanu.Runtime;
using Mosa.FileSystem.VFS;
using Mosa.Runtime.x86;

namespace Abanu.Kernel
{

    public static class Program
    {

        private static VfsFile KeyBoardFifo;

        private const bool TraceFileIO = false;

        public static void Main()
        {
            ApplicationRuntime.Init();

            Files = new List<VfsFile>();
            OpenFiles = new List<OpenFile>();

            KeyBoardFifo = new VfsFile { Path = "/dev/keyboard", Buffer = new FifoFile() };
            Files.Add(KeyBoardFifo);
            Files.Add(new VfsFile { Path = "/dev/console", Buffer = new FifoFile() });

            MessageManager.OnMessageReceived = MessageReceived;
            MessageManager.OnDispatchError = OnDispatchError;

            SysCalls.RegisterService(SysCallTarget.OpenFile);
            SysCalls.RegisterService(SysCallTarget.CreateFifo);
            SysCalls.RegisterService(SysCallTarget.ReadFile);
            SysCalls.RegisterService(SysCallTarget.WriteFile);
            SysCalls.RegisterService(SysCallTarget.GetFileLength);
            SysCalls.RegisterService(SysCallTarget.FStat);

            var targetProcID = SysCalls.GetProcessIDForCommand(SysCallTarget.GetProcessByName);
            GetProcessByNameBuffer = SysCalls.RequestMessageBuffer(4096, targetProcID);

            SysCalls.RegisterService(SysCallTarget.HostCommunication_CreateProcess); // TODO: Obsolete? Consider rename TmpDebug to HostCommunication_CreateProcess
            SysCalls.RegisterService(SysCallTarget.TmpDebug);

            try
            {
                InitHAL.SetupDrivers();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            SysCalls.RegisterInterrupt(33);

            SysCalls.SetServiceStatus(ServiceStatus.Ready);

            while (true)
            {
                SysCalls.ThreadSleep(0);
            }
        }

        public static void OnDispatchError(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        private static MemoryRegion GetProcessByNameBuffer;
        public static void MessageReceived(in SystemMessage msg)
        {
            switch (msg.Target)
            {
                case SysCallTarget.OpenFile:
                    Cmd_OpenFile(msg);
                    break;
                case SysCallTarget.GetFileLength:
                    Cmd_GetFileLength(msg);
                    break;
                case SysCallTarget.FStat:
                    Cmd_FStat(msg);
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
                case SysCallTarget.Interrupt:
                    Cmd_Interrupt(msg);
                    break;
                case SysCallTarget.TmpDebug:
                    if (msg.Arg1 == 1)
                    {
                        var procID = SysCalls.GetProcessByName(GetProcessByNameBuffer, "App.Shell");

                        if (procID == -1)
                            procID = SysCalls.GetProcessByName(GetProcessByNameBuffer, "memory"); // temp name

                        Console.WriteLine("Current ProcID: ");
                        Console.WriteLine(procID.ToString());

                        if (procID > 0)
                            SysCalls.KillProcess(procID);

                        Console.WriteLine("try load proc");
                        HostCommunicator.StartProcess("os/App.Shell.bin");
                        Console.WriteLine("Process Started");
                        MessageManager.Send(new SystemMessage(SysCallTarget.ServiceReturn));
                    }
                    break;
                default:
                    MessageManager.Send(new SystemMessage(SysCallTarget.ServiceReturn));
                    break;
            }
        }

        /// <summary>
        /// General purpose Fifo
        /// </summary>
        internal class FifoStream : Stream
        {
            private byte[] Data;
            private int WritePosition;
            private int ReadPosition;

            private long _Length;
            public override long Length => _Length;

            public override bool CanRead => throw new NotImplementedException("NotImpelmented: CanRead");

            public override bool CanSeek => throw new NotImplementedException("NotImpelmented: CanSeek");

            public override bool CanWrite => throw new NotImplementedException("NotImpelmented: CanWrite");

            public override long Position { get => throw new NotImplementedException("NotImpelmented: GetPosition"); set => throw new NotImplementedException("NotImpelmented: SetPosition"); }

            public FifoStream(int capacity)
            {
                Data = new byte[capacity];
                _Length = 0;
            }

            protected override void Dispose(bool disposing)
            {
                RuntimeMemory.FreeObject(Data);
            }

            public override void Flush()
            {

            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                if (_Length == 0)
                    return 0;

                var cnt = (int)Math.Min(count, Length);
                for (var i = 0; i < cnt; i++)
                {
                    buffer[i] = Data[ReadPosition++];
                    if (ReadPosition >= Data.Length)
                        ReadPosition = 0;
                    _Length--;
                }

                return cnt;
            }

            public override int ReadByte()
            {
                if (_Length == 0)
                    return -1;

                var cnt = (int)Math.Min(1, Length);
                for (var i = 0; i < cnt; i++)
                {
                    var result = Data[ReadPosition++];
                    if (ReadPosition >= Data.Length)
                        ReadPosition = 0;
                    _Length--;
                    return result;
                }

                return -1;
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotImplementedException("NotImpelmented: seek");
            }

            public override void SetLength(long value)
            {
                throw new NotImplementedException("NotImpelmented: SetLength");
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                for (var i = 0; i < count; i++)
                {
                    Data[WritePosition++] = buffer[i];
                    if (WritePosition >= Data.Length)
                        WritePosition = 0;
                    _Length++;
                }
            }

            public override void WriteByte(byte value)
            {
                for (var i = 0; i < 1; i++)
                {
                    Data[WritePosition++] = value;
                    if (WritePosition >= Data.Length)
                        WritePosition = 0;
                    _Length++;
                }
            }
        }

        internal class FifoFile : Stream
        {
            private Stream Data;

            public FifoFile()
            {
                Data = new FifoStream(256);
            }

            public override bool CanRead => Data.CanRead;

            public override bool CanSeek => Data.CanSeek;

            public override bool CanWrite => Data.CanWrite;

            public override long Length => Data.Length;

            public override long Position { get => Data.Position; set => Data.Position = value; }

            public override void Flush()
            {
                Data.Flush();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return Data.Read(buffer, offset, count);
            }

            public override int ReadByte()
            {
                return Data.ReadByte();
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                return Data.Seek(offset, origin);
            }

            public override void SetLength(long value)
            {
                Data.SetLength(value);
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                Data.Write(buffer, offset, count);
            }

            public override void WriteByte(byte value)
            {
                Data.WriteByte(value);
            }
        }

        internal class OpenFile
        {
            public FileHandle Handle;
            public string Path;
            public int ProcessId;
            public Stream Buffer;
        }

        private static List<OpenFile> OpenFiles;

        private static OpenFile FindOpenFileWithDefault(FileHandle handle)
        {

            switch (handle.ToInt32())
            {
                case 0:
                    return EnsurePredefinedHandleIsOpen(handle, "/dev/keyboard");
                case 1:
                case 2:
                    return EnsurePredefinedHandleIsOpen(handle, "/dev/console");
            }

            return FindOpenFile(handle);
        }

        private static OpenFile EnsurePredefinedHandleIsOpen(FileHandle handle, string path)
        {
            var remoteProcessID = SysCalls.GetRemoteProcessID();
            for (var i = 0; i < OpenFiles.Count; i++)
                if (OpenFiles[i].ProcessId == remoteProcessID && OpenFiles[i].Handle == handle)
                    return OpenFiles[i];

            var file = FindFile(path);
            var openFile = new OpenFile()
            {
                Handle = handle,
                Path = path,
                ProcessId = remoteProcessID,
                Buffer = file.Buffer,
            };
            OpenFiles.Add(openFile);
            return openFile;
        }

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
            public Stream Buffer;
            public string Path;
            public int Length;
        }

        private static List<VfsFile> Files;

        internal static VfsFile FindFile(string path)
        {
            for (var i = 0; i < Files.Count; i++)
                if (Files[i].Path == path)
                    return Files[i];

            if (InitHAL.PrimaryFS != null)
            {
                var node = InitHAL.PrimaryFS.Root.Lookup(path);
                if (node != null)
                {
                    var stream = (Stream)node.Open(FileAccess.Read, FileShare.Read);
                    var file = new VfsFile
                    {
                        Path = path,
                        Buffer = stream,
                        Length = (int)stream.Length,
                    };
                    Files.Add(file);
                    return file;
                }
            }

            return null;
        }

        public static void Cmd_Interrupt(in SystemMessage msg)
        {
            var code = Native.In8(0x60);

            //SysCalls.WriteDebugChar('*');
            //SysCalls.WriteDebugChar((char)(byte)code);
            //SysCalls.WriteDebugChar('*');

            // F12
            if (code == 0x58)
            {
                MessageManager.Send(new SystemMessage(SysCallTarget.TmpDebug, 1));
            }

            KeyBoardFifo.Buffer.WriteByte(code);

            MessageManager.Send(new SystemMessage(SysCallTarget.ServiceReturn));
        }

        public static unsafe void Cmd_CreateFiFo(in SystemMessage msg)
        {
            var path = NullTerminatedString.ToString((byte*)msg.Arg1);

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

        public static unsafe void Cmd_CreateMemoryFile(in SystemMessage msg)
        {
            var start = msg.Arg1;
            var length = msg.Arg2;
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

        public static unsafe void Cmd_GetFileLength(in SystemMessage msg)
        {
            var path = NullTerminatedString.ToString((byte*)msg.Arg1);

            if (TraceFileIO)
            {
                Console.Write("Get File Length: ");
                Console.WriteLine(path);
            }

            var file = FindFile(path);
            if (file == null)
            {
                Console.Write("File not found: ");
                //Console.WriteLine(length.ToString("X"));
                Console.WriteLine(path.Length.ToString("X"));
                Console.WriteLine(path);
                Console.WriteLine(">>");
                MessageManager.Send(new SystemMessage(SysCallTarget.ServiceReturn, unchecked((uint)-1)));
                return;
            }

            if (TraceFileIO)
                Console.WriteLine("File Size: " + ((uint)file.Length).ToString("X"));

            MessageManager.Send(new SystemMessage(SysCallTarget.ServiceReturn, (uint)file.Length));
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Stat
        {
            public int Dev;
            public ulong Ino;
            public int Mode;
            public int Nlink;
            public int Uid;
            public int Gid;
            public int Rdev;
            public int Size;
            public int Blksize;
            public int Blocks;
            public int Atime;
            public int Mtime;
            public int Ctime;
        }

        public static unsafe void Cmd_FStat(in SystemMessage msg)
        {
            var fd = msg.Arg1;
            var stat = (Stat*)msg.Arg2;

            if (TraceFileIO)
            {
                Console.Write("Get FStat: ");
                Console.WriteLine(fd.ToString());
            }

            //var file = FindFile(path);
            //if (file == null)
            //{
            //    Console.Write("File not found: ");
            //    //Console.WriteLine(length.ToString("X"));
            //    Console.WriteLine(path.Length.ToString("X"));
            //    Console.WriteLine(path);
            //    Console.WriteLine(">>");
            //    MessageManager.Send(new SystemMessage(SysCallTarget.ServiceReturn, unchecked((uint)-1)));
            //    return;
            //}

            stat->Size = 0;

            //if (TraceFileIO)
            //    Console.WriteLine("File Size: " + ((uint)file.Length).ToString("X"));

            MessageManager.Send(new SystemMessage(SysCallTarget.ServiceReturn, 0));
        }

        public static unsafe void Cmd_OpenFile(in SystemMessage msg)
        {

            //var addr = msg->Arg1;
            //var str = (NullTerminatedString*)addr;
            //var path = str->ToString();

            //var path = ((NullTerminatedString*)msg->Arg1)->ToString();
            var path = NullTerminatedString.ToString((byte*)msg.Arg1);

            if (TraceFileIO)
            {
                Console.Write("Open File: ");
                Console.WriteLine(path);
            }

            var file = FindFile(path);
            if (file == null)
            {
                Console.Write("File not found: ");
                //Console.WriteLine(length.ToString("X"));
                Console.WriteLine(path.Length.ToString("X"));
                Console.WriteLine(path);
                Console.WriteLine(">>");
                MessageManager.Send(new SystemMessage(SysCallTarget.ServiceReturn, FileHandle.Zero));
                return;
            }

            var openFile = new OpenFile()
            {
                Handle = ++lastHandle,
                Path = path,
                ProcessId = SysCalls.GetRemoteProcessID(),
                Buffer = file.Buffer,
            };
            OpenFiles.Add(openFile);

            if (TraceFileIO)
                Console.WriteLine("Created Handle: " + ((uint)openFile.Handle).ToString("X"));

            MessageManager.Send(new SystemMessage(SysCallTarget.ServiceReturn, openFile.Handle));
        }

        public static unsafe void Cmd_ReadFile(in SystemMessage msg)
        {
            if (TraceFileIO)
                Console.WriteLine("Read Handle: " + msg.Arg1.ToString("X"));

            var openFile = FindOpenFileWithDefault((int)msg.Arg1);
            if (openFile == null)
            {
                Console.WriteLine("Handle not found: " + msg.Arg1.ToString());
                MessageManager.Send(new SystemMessage(SysCallTarget.ServiceReturn));
                return;
            }

            var data = (byte*)msg.Arg2;
            var length = msg.Arg3;
            var gotBytes = openFile.Buffer.Read(data, (int)length);
            MessageManager.Send(new SystemMessage(SysCallTarget.ServiceReturn, (uint)gotBytes));
        }

        public static unsafe void Cmd_WriteFile(in SystemMessage msg)
        {
            if (TraceFileIO)
                Console.WriteLine("Write Handle: " + msg.Arg1.ToString("X"));

            var openFile = FindOpenFileWithDefault((int)msg.Arg1);
            if (openFile == null)
            {
                Console.WriteLine("Handle not found: " + msg.Arg1.ToString());
                MessageManager.Send(new SystemMessage(SysCallTarget.ServiceReturn));
                return;
            }

            var data = (byte*)msg.Arg2;
            var length = msg.Arg3;
            openFile.Buffer.Write(data, (int)length);
            MessageManager.Send(new SystemMessage(SysCallTarget.ServiceReturn, length));
        }

    }
}
