// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;

namespace Abanu.Test.Console
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            System.Console.WriteLine(Environment.Is64BitProcess);
            System.Console.WriteLine(IntPtr.Size);
            System.Console.WriteLine("Hello World!");
            //new AllocTest().run();
            new Alloc6.Tester().run();
            //new Abanu.Test.malloc4.Tester().run();

            System.Console.ReadLine();
        }
    }
}
