// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lonos.Kernel.Core.x86
{
    public static class DummyClass
    {
        public static void DummyCall()
        {
            Core.DummyClass.DummyCall();
        }
    }
}
