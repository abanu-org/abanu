// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable SA1300 // Element should begin with upper-case letter
namespace Lonos.Kernel.Core.x86
#pragma warning restore SA1300 // Element should begin with upper-case letter
{
    public static class DummyClass
    {
        public static void DummyCall()
        {
            Core.DummyClass.DummyCall();
            var t = typeof(Mosa.Runtime.GC);
            var t2 = typeof(Mosa.Plug.Korlib.System.Threading.x86.InterlockedPlug);
            var t3 = typeof(Mosa.Runtime.x86.Internal);
        }
    }
}
