// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Abanu.Kernel.Core;
using Abanu.Runtime;
using Mosa.Runtime.x86;

namespace Abanu.Kernel
{

    public static class Program
    {

        private static ThreadLocal<uint> Local;

        public static unsafe void Main()
        {
            ApplicationRuntime.Init();

            //var result = MessageManager.Send(SysCallTarget.ServiceFunc1, 55);

            SysCalls.WriteDebugChar('=');
            SysCalls.WriteDebugChar('/');
            SysCalls.WriteDebugChar('*');

            var targetProcessId = SysCalls.GetProcessIDForCommand(SysCallTarget.OpenFile);
            var buf = SysCalls.RequestMessageBuffer(4096, targetProcessId);
            var keyboardHandle = SysCalls.OpenFile(buf, "/dev/keyboard");

            var consoleHandle = SysCalls.OpenFile(buf, "/dev/console");

            var consoleFile = new FileStream(consoleHandle);
            var con = new ConsoleClient(consoleFile);

            con.Reset();
            con.SetForegroundColor(7);
            con.SetBackgroundColor(0);
            con.ApplyDefaultColor();
            con.Clear();
            con.SetCursor(0, 0);
            con.Write("kl\n");

            Local = new ThreadLocal<uint>();
            Local.Value = 0xFF998877;
            con.WriteLine("TLS: " + Local.Value.ToString("x"));

            for (uint i = 0; i < ApplicationRuntime.ElfSections.SectionHeaderCount; i++)
            {
                var section = ApplicationRuntime.ElfSections.GetSectionHeader(i);
                var name = ApplicationRuntime.ElfSections.GeSectionName(section);
                con.WriteLine(name);
            }

            while (true)
            {
                SysCalls.ThreadSleep(0);

                //SysCalls.WriteDebugChar('~');
                var gotBytes = SysCalls.ReadFile(keyboardHandle, buf);
                if (gotBytes > 0)
                {
                    for (var byteIdx = 0; byteIdx < gotBytes; byteIdx++)
                    {
                        var bufPtr = (byte*)buf.Start;
                        var key = bufPtr[byteIdx];

                        // F9
                        if (key == 0x43)
                        {
                            StartProc("DSPSRV.BIN");
                            continue;
                        }

                        // F10
                        if (key == 0x44)
                        {
                            StartProc("GUIDEMO.BIN");
                            continue;
                        }

                        var s = key.ToString("x");
                        //for (var i = 0; i < s.Length; i++)
                        //    SysCalls.WriteDebugChar(s[i]);
                        //SysCalls.WriteDebugChar(' ');
                        for (var i = 0; i < s.Length; i++)
                            con.Write(s[i]);
                        con.Write(' ');
                    }
                }
                //SysCalls.WriteDebugChar('?');
            }
        }

        private static unsafe void StartProc(string name)
        {
            var fileServiceProc = SysCalls.GetProcessIDForCommand(SysCallTarget.GetFileLength);
            var nameBuf = SysCalls.RequestMessageBuffer(4096, fileServiceProc);
            var length = SysCalls.GetFileLength(nameBuf, name);
            if (length < 0)
                return;

            Console.WriteLine("Loading App: " + name);
            Console.WriteLine("Length: " + length.ToString());

            var targetProcessStartProc = SysCalls.GetProcessIDForCommand(SysCallTarget.CreateMemoryProcess);
            var fileBuf = SysCalls.RequestMessageBuffer((uint)length, targetProcessStartProc);

            var transferBuf = SysCalls.RequestMessageBuffer(4096, fileServiceProc);

            var handle = SysCalls.OpenFile(nameBuf, name);
            uint pos = 0;

            var filePtr = (byte*)fileBuf.Start;
            var transferPtr = (byte*)transferBuf.Start;
            SysCalls.SetThreadPriority(30);
            while (true)
            {
                var gotBytes = SysCalls.ReadFile(handle, transferBuf);

                //Console.Write(gotBytes.ToString());

                //if (gotBytes == 132)
                //{
                //    name.IndexOf("");
                //}

                if (gotBytes <= 0)
                    break;

                for (var i = 0; i < gotBytes; i++)
                {
                    filePtr[pos++] = transferPtr[i];
                }
                //Console.Write(".");
            }
            SysCalls.SetThreadPriority(0);

            Console.WriteLine("CreateProc...");
            SysCalls.CreateMemoryProcess(fileBuf, (uint)length);
        }

        public static unsafe void OnDispatchError(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

    }
}
