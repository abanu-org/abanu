// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System;
using Abanu.Kernel.Core.Collections;
using Abanu.Kernel.Core.MemoryManagement;
using Abanu.Kernel.Core.Processes;
using Abanu.Kernel.Core.Scheduling;
using Abanu.Kernel.Core.Tasks;

#pragma warning disable CA2000 // Dispose objects before losing scope

namespace Abanu.Kernel.Core
{

    public static class ThreadTests
    {

        public static void RunTests()
        {
        }

        public static void StartTestThreads()
        {
            Scheduler.CreateThread(ProcessManager.System, new ThreadStartOptions(Thread0) { DebugName = "KernelThread0", Priority = -5 }).Start();

            var userProc = ProcessManager.CreateEmptyProcess(new ProcessCreateOptions { User = false });
            userProc.Path = "/buildin/testproc";
            Scheduler.CreateThread(userProc, new ThreadStartOptions(Thread1) { AllowUserModeIOPort = true, DebugName = "UserThread1", Priority = -5 });
            Scheduler.CreateThread(userProc, new ThreadStartOptions(Thread2) { AllowUserModeIOPort = true, DebugName = "UserThread2", Priority = -5 });
            userProc.Start();
        }

        private static void Thread0()
        {
            KernelMessage.WriteLine("Thread0: Enter");
            uint tsLast = 0;
            //var chars = new char[] { '-', '/', '|', '\\' };
            var chars = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            var charIdx = 0;
            while (true)
            {
                Scheduler.Sleep(0);
                var ts = PerformanceCounter.GetReadableCounter();
                if (ts - tsLast < 1000)
                    continue;

                Screen.SetChar(chars[charIdx], 0, 30, ConsoleColor.White, ConsoleColor.Red);
                charIdx++;
                if (charIdx >= chars.Length)
                    charIdx = 0;
                tsLast = ts;
            }
            KernelMessage.WriteLine("Thread0: Finished");
        }

        private static void Thread1()
        {
            KernelMessage.WriteLine("Thread1: Enter");
            uint tsLast = 0;
            //var chars = new char[] { '-', '/', '|', '\\' };
            var chars = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            var charIdx = 0;
            while (true)
            {
                Scheduler.Sleep(0);
                var ts = PerformanceCounter.GetReadableCounter();
                if (ts - tsLast < 1000)
                    continue;

                Screen.SetChar(chars[charIdx], 0, 32, ConsoleColor.White, ConsoleColor.Green);
                charIdx++;
                if (charIdx >= chars.Length)
                    charIdx = 0;

                //PhysicalPageManager.DumpPages();
                //PhysicalPageManager.DumpStats();
                //VirtualPageManager.DumpStats();

                tsLast = ts;
            }
            KernelMessage.WriteLine("Thread1: Finished");
        }

        private static void Thread2()
        {
            KernelMessage.WriteLine("Thread2: Enter");
            uint tsLast = 0;
            //var chars = new char[] { '-', '/', '|', '\\' };
            var chars = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            var charIdx = 0;
            var i = 0;
            while (true)
            {
                Scheduler.Sleep(0);
                var ts = PerformanceCounter.GetReadableCounter();
                if (ts - tsLast < 1000)
                    continue;

                Screen.SetChar(chars[charIdx], 0, 34, ConsoleColor.White, ConsoleColor.Red);
                charIdx++;
                if (charIdx >= chars.Length)
                    charIdx = 0;
                tsLast = ts;
                if (i++ > 10)
                    break;
            }
            KernelMessage.WriteLine("Thread2: Finished");
            //while (true)
            //    i++;
        }

        /// <summary>
        /// Signal the Integration tests, that Test was successful.
        /// </summary>
        public static void TriggerTestPassed()
        {
            Uninterruptible.Execute(() => KernelMessage.WriteLine(KConfig.SelfTestPassedMarker));
        }

    }
}
