using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lonos.Kernel.Loader.x86
{
    public static class DummyClass
    {
        public static void DummyCall()
        {
            Loader.DummyClass.DummyCall();
        }
    }
}
