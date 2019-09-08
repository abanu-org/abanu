using lonos.Kernel.Core;
using Mosa.Runtime.Plug;
using System;

namespace lonos.Runtime
{

    public static class ApplicationRuntime
    {

        public static void Init()
        {
            initialMemoryNextAddr = SysCalls.RequestMemory(3 * 1024 * 1024);
            InitializAssembly();
        }

        #region InitAssembly

        private static void InitializAssembly()
        {
            Mosa.Runtime.StartUp.InitializeAssembly();
        }

        /// <summary>
        /// This method needs be be directly after InitializeAssembly, because of missing prologue and epilogue,
        /// otherwiese it will "call" (move to) the next available method, and with missing method arguments.
        /// </summary>
        private static void BugFixDummyCall()
        {
        }

        #endregion

        [Plug("Mosa.Runtime.GC::AllocateMemory")]
        private static unsafe IntPtr _AllocateMemory(uint size)
        {
            return (IntPtr)AllocateMemory(size);
        }

        private static uint initialMemoryNextAddr;
        private static uint AllocateMemory(uint size)
        {
            var retAddr = initialMemoryNextAddr;
            initialMemoryNextAddr += size;
            return retAddr;
        }

        private static void Dummy()
        {
            //This is a dummy call, that get never executed.
            //Its requied, because we need a real reference to Mosa.Runtime.x86
            //Without that, the .NET compiler will optimize that reference away
            //if its nowhere used. Than the Compiler dosnt know about that Refernce
            //and the Compilation will fail
            Mosa.Runtime.x86.Internal.GetStackFrame(0);
        }

    }

}
