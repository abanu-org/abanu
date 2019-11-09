// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Abanu.Kernel;
using Abanu.Kernel.Core;
using Abanu.Kernel.Core.Elf;
using Mosa.Runtime;
using Mosa.Runtime.x86;

namespace Abanu.Runtime
{

    public static class ApplicationRuntime
    {

        internal static void DummyReference()
        {
            var t = typeof(Mosa.Runtime.GC);
            var t2 = typeof(Mosa.Plug.Korlib.System.Threading.x86.InterlockedPlug);
            var t3 = typeof(Mosa.Runtime.x86.Internal);
        }

        private static int _CurrentProcessID;
        public static int CurrentProcessID
        {
            get
            {
                if (_CurrentProcessID == 0)
                    _CurrentProcessID = SysCalls.GetCurrentProcessID();
                return _CurrentProcessID;
            }
        }

        public static void Init()
        {
            _CurrentProcessID = 0;

            RuntimeMemory.SetupEarlyStartup();
            RuntimeMemory.SetupAllocator();
            InitializAssembly();
            SetupElfSections();
            InitThreadLocalStorage();
            //Mosa.Runtime.StartUp.InitializeRuntimeMetadata();
        }

        private static unsafe void SetupElfSections()
        {
            ElfSectionsInternal = *((ElfSections*)SysCalls.GetElfSectionsAddress());
            ElfSectionsInternal.Init();

            ElfSections = new RuntimeElfSectionCollection((int)ElfSectionsInternal.SectionHeaderCount);
            for (uint i = 0; i < ElfSectionsInternal.SectionHeaderCount; i++)
            {
                var section = ElfSectionsInternal.GetSectionHeader(i);
                var name = ElfSectionsInternal.GeSectionName(section);
                var sec = new RuntimeElfSection(NullTerminatedString.ToString(name), section, ElfSectionsInternal.GetSectionPhysAddr(section));
                ElfSections.Add(sec);
            }
        }

        private static unsafe void InitThreadLocalStorage()
        {
            // Position 0 is reserved for ThreadLocalStorageBlock
            Thread.NextSlotPosition = Addr.Size;
            InitCurrentThread();
        }

        private static unsafe void InitCurrentThread()
        {
            var tls = (ThreadLocalStorageBlock*)SysCalls.RequestMemory(4096);
            tls->ThreadID = SysCalls.GetCurrentThreadID();
            var th = new Thread(tls);
            tls->ThreadPtr = (uint)(void*)Intrinsic.GetObjectAddress(th);
            SysCalls.SetThreadStorageSegmentBase(tls->ThreadPtr);
        }

        internal static unsafe ThreadLocalStorageBlock* GetThreadLocalStorageBlock()
        {
            var addr = (void*)Thread.GetThreadLocalStorage(0);
            return (ThreadLocalStorageBlock*)addr;
        }

        public static unsafe void Exit()
        {
            Exit(0);
        }

        public static unsafe void Exit(int exitCode)
        {
            SysCalls.KillProcess(CurrentProcessID);
        }

        #region InitAssembly

        private static void InitializAssembly()
        {
            StartUp.InitializeAssembly();
        }

        /// <summary>
        /// This method needs be be directly after InitializeAssembly, because of missing prologue and epilogue,
        /// otherwiese it will "call" (move to) the next available method, and with missing method arguments.
        /// </summary>
        private static void BugFixDummyCall()
        {
        }

        #endregion

        private static ElfSections ElfSectionsInternal;
        public static RuntimeElfSectionCollection ElfSections;

        private static void Dummy()
        {
            // This is a dummy call, that get never executed.
            // Its required, because we need a real reference to Mosa.Runtime.x86
            // Without that, the .NET compiler will optimize that reference away
            // if its nowhere used. Than the Compiler doesn't know about that Reference
            // and the Compilation will fail
            Mosa.Runtime.x86.Internal.ExceptionHandler();
        }

        public static MemoryAllocation RequestMessageBuffer(int size, int targetProcessID)
        {
            var buf = SysCalls.RequestMessageBuffer((uint)size, targetProcessID);
            return new MemoryAllocation(buf);
        }

    }

}
