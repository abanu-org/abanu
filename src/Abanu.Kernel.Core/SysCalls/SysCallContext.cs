// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System;

namespace Abanu.Kernel.Core.SysCalls
{

    public struct SysCallContext
    {
        public SysCallCallingType CallingType;

        /// <summary>
        /// If true, the current command will be debugged
        /// </summary>
        public bool Debug;
    }

}
