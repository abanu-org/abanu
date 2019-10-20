// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;

namespace Lonos.Kernel
{
    [Flags]
    public enum ConsoleCharAttributes
    {
        None = 0,
        Bold = 1,
    }

}
