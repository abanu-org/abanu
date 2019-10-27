// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Abanu.Kernel.Core;
using Abanu.Runtime;
using Mosa.Runtime.x86;

namespace Abanu.Kernel
{

    public static class Program
    {

        public static unsafe void Main()
        {
            ApplicationRuntime.Init();

            //var result = MessageManager.Send(SysCallTarget.ServiceFunc1, 55);

            SysCalls.WriteDebugChar('=');
            SysCalls.WriteDebugChar('/');
            SysCalls.WriteDebugChar('*');

            var targetProcessId = SysCalls.GetProcessIDForCommand(SysCallTarget.OpenFile);
            var buf = SysCalls.RequestMessageBuffer(4096, targetProcessId);
            var kb = SysCalls.OpenFile(buf, "/dev/keyboard");

            var con = new ConsoleClient();
            con.Init();
            //con.Write("\x001B[37;42m\x001B[8]");
            //con.Write("abc\x001B[2Jgh\x001B[37;42mjk");

            con.Reset();
            con.SetForegroundColor(7);
            con.SetBackgroundColor(0);
            con.ApplyDefaultColor();
            con.Clear();
            con.SetCursor(0, 0);
            con.Write("kl\n");

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
                var gotBytes = SysCalls.ReadFile(kb, buf);
                if (gotBytes > 0)
                {
                    for (var byteIdx = 0; byteIdx < gotBytes; byteIdx++)
                    {
                        var bufPtr = (byte*)buf.Start;
                        var key = bufPtr[byteIdx];
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

        public static unsafe void OnDispatchError(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

    }
}
