// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
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

        public static void Main()
        {
            try
            {
                ApplicationRuntime.Init();

                //var result = MessageManager.Send(SysCallTarget.ServiceFunc1, 55);

                SysCalls.WriteDebugChar('=');
                SysCalls.WriteDebugChar('/');
                SysCalls.WriteDebugChar('*');

                var conStream = File.Open("/dev/console");

                var con = new ConsoleClient(conStream);

                con.Reset();
                con.SetForegroundColor(7);
                con.SetBackgroundColor(0);
                con.ApplyDefaultColor();
                con.Clear();
                con.SetCursor(0, 0);
                con.Write("kl\n");

                for (int i = 0; i < ApplicationRuntime.ElfSections.Count; i++)
                    con.WriteLine(ApplicationRuntime.ElfSections[i].Name);

                using (var kbStream = File.Open("/dev/keyboard"))
                {
                    while (true)
                    {
                        SysCalls.ThreadSleep(0);

                        var num = kbStream.ReadByte();
                        if (num >= 0)
                        {
                            var key = (byte)num;

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
                        //SysCalls.WriteDebugChar('?');
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void StartProc(string name)
        {
            var fileServiceProc = SysCalls.GetProcessIDForCommand(SysCallTarget.GetFileLength);
            var nameBuf = SysCalls.RequestMessageBuffer(4096, fileServiceProc);
            var length = SysCalls.GetFileLength(nameBuf, name);
            if (length < 0)
                return;

            Console.WriteLine("Loading App: " + name);
            Console.WriteLine("Length: " + length.ToString());

            var targetProcessStartProc = SysCalls.GetProcessIDForCommand(SysCallTarget.CreateMemoryProcess);
            var fileBuf = ApplicationRuntime.RequestMessageBuffer(length, targetProcessStartProc);

            var transferBuf = new byte[4096];

            var handle = File.Open(name);
            int pos = 0;

            SysCalls.SetThreadPriority(30);
            while (true)
            {
                var gotBytes = handle.Read(transferBuf, 0, transferBuf.Length);

                if (gotBytes <= 0)
                    break;

                fileBuf.Write(transferBuf, 0, pos, gotBytes);
                pos += gotBytes;

                //for (var i = 0; i < gotBytes; i++)
                //{
                //    fileBuf.SetByte(pos++, transferBuf[i]);
                //}
            }
            SysCalls.SetThreadPriority(0);

            Console.WriteLine("CreateProc...");
            SysCalls.CreateMemoryProcess(fileBuf.Region, (uint)length);
        }

        public static void OnDispatchError(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

    }
}
