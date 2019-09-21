// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Diagnostics;

namespace Lonos.Tools.Build
{
    public class ProcessResult : CommandResult, IDisposable
    {
        public Process Process;

        public ProcessResult(Process proc)
        {
            Process = proc;
        }

        public void Dispose()
        {
            try
            {
                Process?.Dispose();
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
