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

        public static void Main()
        {
            ApplicationRuntime.Init();

            Service.Setup();

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
                    Service.Cmd_OpenFile(msg);
                    break;
                case SysCallTarget.GetFileLength:
                    Service.Cmd_GetFileLength(msg);
                    break;
                case SysCallTarget.FStat:
                    Service.Cmd_FStat(msg);
                    break;
                case SysCallTarget.WriteFile:
                    Service.Cmd_WriteFile(msg);
                    break;
                case SysCallTarget.ReadFile:
                    Service.Cmd_ReadFile(msg);
                    break;
                case SysCallTarget.CreateFifo:
                    Service.Cmd_CreateFiFo(msg);
                    break;
                case SysCallTarget.Interrupt:
                    Service.Cmd_Interrupt(msg);
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
            MessageManager.Send(new SystemMessage(SysCallTarget.ServiceReturn));
        }

    }

}
