// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using Lonos.Kernel.Loader;

namespace Lonos.OS.Loader.x86
{
    public static class Start
    {
        public static void Main()
        {
            Kernel.Loader.LoaderStart.Main();
            Kernel.Loader.x86.DummyClass.DummyCall();
            while (true)
            {
            }
        }
    }
}
