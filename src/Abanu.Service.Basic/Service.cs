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

    internal static class Service
    {

        private static VfsFile KeyBoardFifo;

        private const bool TraceFileIO = false;
        private static List<OpenFile> OpenFiles;

        public static void Setup()
        {
            Files = new List<VfsFile>();
            OpenFiles = new List<OpenFile>();

            Files.Add(new VfsFile { Path = "/dev/null", Buffer = new NullStream() });

            KeyBoardFifo = new VfsFile { Path = "/dev/keyboard", Buffer = new FifoFile() };
            Files.Add(KeyBoardFifo);

            Files.Add(new VfsFile { Path = "/dev/console", Buffer = new FifoFile() });
        }

        private static OpenFile FindOpenFileWithDefault(int processID, FileHandle handle)
        {

            switch (handle.ToInt32())
            {
                case 0:
                    return EnsurePredefinedHandleIsOpen(processID, handle, "/dev/keyboard");
                case 1:
                case 2:
                    return EnsurePredefinedHandleIsOpen(processID, handle, "/dev/console");
            }

            return FindOpenFile(handle);
        }

        private static OpenFile EnsurePredefinedHandleIsOpen(int processID, FileHandle handle, string path)
        {
            for (var i = 0; i < OpenFiles.Count; i++)
                if (OpenFiles[i].ProcessId == processID && OpenFiles[i].Handle == handle)
                    return OpenFiles[i];

            var file = FindFile(path);
            var openFile = new OpenFile()
            {
                Handle = handle,
                Path = path,
                ProcessId = processID,
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

        public static void Cmd_CreateStandartInputOutput(in SystemMessage msg)
        {
            var processID = (int)msg.Arg1;

            CreateStandartInputOutput(processID, FileHandle.StandaradInput);
            CreateStandartInputOutput(processID, FileHandle.StandaradOutput);
            CreateStandartInputOutput(processID, FileHandle.StandaradError);

            MessageManager.Send(new SystemMessage(SysCallTarget.ServiceReturn));
        }

        private static void CreateStandartInputOutput(int processID, FileHandle defaultHandle)
        {
            var file = FindFile("/dev/null");
            var openFile = new OpenFile()
            {
                Handle = defaultHandle,
                Path = "/proc/" + processID + "/" + defaultHandle.ToInt32().ToString(),
                ProcessId = processID,
                Buffer = file.Buffer,
            };
            OpenFiles.Add(openFile);
        }

        public static unsafe void Cmd_SetStandartInputOutput(in SystemMessage msg)
        {
            var procId = (int)msg.Arg1;
            var handle = (FileHandle)(int)msg.Arg2;
            var path = NullTerminatedString.ToString((byte*)msg.Arg3);
            SetStandartInputOutput(procId, handle, path);
        }

        private static void SetStandartInputOutput(int processID, FileHandle defaultHandle, string targetPath)
        {
            OpenFile(processID, targetPath, defaultHandle);
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

        public static unsafe void Cmd_FStat(in SystemMessage msg)
        {
            var fd = msg.Arg1;
            var stat = (Stat*)msg.Arg2;

            if (TraceFileIO)
            {
                Console.Write("Get FStat: ");
                Console.WriteLine(fd.ToString());
            }

            stat->Size = 0;

            MessageManager.Send(new SystemMessage(SysCallTarget.ServiceReturn, 0));
        }

        public static unsafe void Cmd_OpenFile(in SystemMessage msg)
        {
            var path = NullTerminatedString.ToString((byte*)msg.Arg1);

            var processID = SysCalls.GetRemoteProcessID();

            var openFile = OpenFile(processID, path);
            if (openFile == null)
                return;

            MessageManager.Send(new SystemMessage(SysCallTarget.ServiceReturn, openFile.Handle));
        }

        public static unsafe void Cmd_CreateProcessFromFile(in SystemMessage msg)
        {
            var path = NullTerminatedString.ToString((byte*)msg.Arg1);

            // buffer....
            MemoryRegion buf;

            var procID = SysCalls.CreateMemoryProcess(buf, buf.Size);
            SysCalls.CreateMemoryProcess(buf, buf.Size);

            MessageManager.Send(new SystemMessage(SysCallTarget.ServiceReturn));
        }

        /// <summary>
        /// Opens a file
        /// </summary>
        /// <param name="processID">Owner of the handle</param>
        /// <param name="path">File to open</param>
        /// <param name="handle">force specific handle number</param>
        public static OpenFile OpenFile(int processID, string path, FileHandle handle = default)
        {
            if (TraceFileIO)
            {
                Console.Write("Open File: ");
                Console.WriteLine(path);
            }

            var file = FindFile(path);
            if (file == null)
            {
                Console.Write("File not found: ");
                Console.WriteLine(path.Length.ToString("X"));
                Console.WriteLine(path);
                Console.WriteLine(">>");
                MessageManager.Send(new SystemMessage(SysCallTarget.ServiceReturn, FileHandle.Zero));
                return null;
            }

            if (handle == default)
                handle = ++lastHandle;

            var openFile = new OpenFile()
            {
                Handle = handle,
                Path = "/proc/" + processID + "/" + handle.ToInt32().ToString(),
                ProcessId = processID,
                Buffer = file.Buffer,
            };
            OpenFiles.Add(openFile);

            if (TraceFileIO)
                Console.WriteLine("Created Handle: " + ((uint)openFile.Handle).ToString("X"));

            return openFile;
        }

        public static unsafe void Cmd_ReadFile(in SystemMessage msg)
        {
            if (TraceFileIO)
                Console.WriteLine("Read Handle: " + msg.Arg1.ToString("X"));

            var remoteProcessID = SysCalls.GetRemoteProcessID();

            var openFile = FindOpenFileWithDefault(remoteProcessID, (int)msg.Arg1);
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

            var remoteProcessID = SysCalls.GetRemoteProcessID();

            var openFile = FindOpenFileWithDefault(remoteProcessID, (int)msg.Arg1);
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
