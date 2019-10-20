// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Lonos;
using Lonos.Kernel.Core;
using Lonos.Runtime;

namespace Lonos.Kernel
{

    public static class ConsoleServerConstants
    {
        public const char ESC = (char)27;
        public static readonly string Reset = "c";
        public static readonly string ClarScreen = "[2J";
        public static readonly string ModesOff = "[0m";
        public static readonly string BoldOn = "[1m";
    }

}
