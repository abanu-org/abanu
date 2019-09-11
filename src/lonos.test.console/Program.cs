// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;

namespace Lonos.test.console
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Console.WriteLine(Environment.Is64BitProcess);
            Console.WriteLine(IntPtr.Size);
            Console.WriteLine("Hello World!");
            //new AllocTest().run();

            new Lonos.test.malloc4.Tester().run();

            Console.ReadLine();
        }
    }
}
