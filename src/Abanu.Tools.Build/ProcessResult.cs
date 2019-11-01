// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Timers;

namespace Abanu.Tools.Build
{
    public class ProcessResult : CommandResult, IDisposable
    {
        public Process Process;

        public ProcessResult(Process proc, TimeSpan? timeout = null)
        {
            Process = proc;
            if (timeout != null)
            {
                var ts = (TimeSpan)timeout;
                if (ts != TimeSpan.Zero)
                {
                    Timer = new Timer(((TimeSpan)timeout).TotalMilliseconds);
                    Timer.Elapsed += ElapsedEventHandler;
                }
            }
        }

        private Timer Timer;

        private void ElapsedEventHandler(object sender, ElapsedEventArgs e)
        {
            Timer.Stop();
            Console.WriteLine("Timeout");
            Environment.Exit(1);
        }

        public void Dispose()
        {
            try
            {
                if (Timer != null)
                    Timer.Stop();

                Process?.Dispose();

                if (Timer != null)
                {
                    var t = Timer;
                    Timer = null;
                    t.Dispose();
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                Process = null;
            }
        }
    }

}
