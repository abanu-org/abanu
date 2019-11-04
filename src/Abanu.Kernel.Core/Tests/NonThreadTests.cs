// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using Abanu.Kernel.Core.Collections;
using Abanu.Kernel.Core.MemoryManagement;

namespace Abanu.Kernel.Core
{

    public static class NonThreadTests
    {

        public static void RunTests()
        {
            var ar = new KList<uint>();
            ar.Add(44);
            ar.Add(55);
            KernelMessage.WriteLine("CNT: {0}", ManagedMemoy.AllocationCount);
            foreach (var num in ar)
            {
                KernelMessage.WriteLine("VAL: {0}", num);
            }
            KernelMessage.WriteLine("CNT: {0}", ManagedMemoy.AllocationCount);
            ar.Destroy();

            KernelMessage.WriteLine("Phys Pages free: {0}", PhysicalPageManager.FreePages);

            for (var i = 0; i < 10000; i++)
            {
                var s = new int[] { 1, 2, 3, 4, };
                s[1] = 5;
                Memory.FreeObject(s);
            }
            KernelMessage.WriteLine("Phys Pages free: {0}", PhysicalPageManager.FreePages);
            //Memory.FreeObject(s);

        }
    }

}
