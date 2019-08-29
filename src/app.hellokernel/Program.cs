using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace lonos.Kernel
{
    public static class Program
    {
        public static void Main()
        {
            Test1();
            while (true) { }
        }

        [DllImport("asm/app.helloworld.test1.o", EntryPoint = "Test1")]
        public extern static void Test1();
    }
}
