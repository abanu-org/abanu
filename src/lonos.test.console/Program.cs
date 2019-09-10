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
