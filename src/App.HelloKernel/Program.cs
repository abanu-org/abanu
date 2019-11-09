// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Abanu.Kernel.Core;
using Abanu.Runtime;
using Mosa.DeviceDriver;
using Mosa.DeviceDriver.ISA;
using Mosa.DeviceDriver.ScanCodeMap;
using Mosa.DeviceSystem;
using Mosa.DeviceSystem.PCI;
using Mosa.FileSystem.FAT;
using Mosa.Runtime.x86;

namespace Abanu.Kernel
{

    public static class Program
    {

        public static unsafe void Main()
        {
            ApplicationRuntime.Init();

            //MessageManager.OnMessageReceived = MessageReceived;

            //SysCalls.SetServiceStatus(ServiceStatus.Ready);

            while (true)
            {
                //Serial.Write(port, (byte)'M');
                Thread.Sleep(0);
            }
        }

        //public static unsafe void MessageReceived(SystemMessage* msg)
        //{
        //}

    }

}
