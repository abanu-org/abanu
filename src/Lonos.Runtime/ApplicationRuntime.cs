// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using Lonos.Kernel.Core;

namespace Lonos.Runtime
{

    public static class ApplicationRuntime
    {

        internal static void DummyReference()
        {
            var t = typeof(Mosa.Runtime.GC);
            var t2 = typeof(Mosa.Plug.Korlib.System.Threading.x86.InterlockedPlug);
            var t3 = typeof(Mosa.Runtime.x86.Internal);
        }

        public static void Init()
        {
            RuntimeMemory.SetupEarlyStartup();
            InitializAssembly();
            RuntimeMemory.SetupAllocator();
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

        private static void Dummy()
        {
            // This is a dummy call, that get never executed.
            // Its required, because we need a real reference to Mosa.Runtime.x86
            // Without that, the .NET compiler will optimize that reference away
            // if its nowhere used. Than the Compiler doesn't know about that Reference
            // and the Compilation will fail
            Mosa.Runtime.x86.Internal.GetStackFrame(0);
        }

    }

}
