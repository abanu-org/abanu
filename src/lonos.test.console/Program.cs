using System;

namespace lonos.test.console
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Console.WriteLine(Environment.Is64BitProcess);
            Console.WriteLine(IntPtr.Size);
            Console.WriteLine("Hello World!");
            new AllocTest().run();
            Console.ReadLine();
        }
    }
}
