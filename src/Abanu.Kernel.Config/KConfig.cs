// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

#pragma warning disable SA1300 // Element should begin with upper-case letter

namespace Abanu.Kernel.Core
{

    /// <summary>
    /// Static Kernel Configuration.
    /// Because Members are accessed before Runtime-Initialization, only constants and Readonly-Properties are allowed
    /// </summary>
    public static class KConfig
    {
        /// <summary>
        /// Sets the target CPU-Type. For Feature-Detection.
        /// Hint: Official Bochs emulator is x86 only.
        /// </summary>
        public const KConfigCpu Cpu = KConfigCpu.x86;

        /// <summary>
        /// Sets the compile architecture.
        /// </summary>
        public const KConfigCpu Arch = KConfigCpu.x86;

        /// <summary>
        /// The Name of Kernel Entry Method. It's used by the Loader to determine it's address.
        /// </summary>
        public static readonly string KernelEntryName = "Abanu.OS.Core.x86.Start::Main()";

        /// <summary>
        /// If turned on, only Pages with Writable=1 are writable.
        /// </summary>
        public const bool UseKernelMemoryProtection = true;

        /// <summary>
        /// Use PAE-Paging instead of 32 Bit Paging.
        /// </summary>
        public const bool UsePAE = true && Cpu == KConfigCpu.x64;

        /// <summary>
        /// If tuned on, only Pages with NXE=1 are executable.
        /// Requires PAE Paging.
        /// Requires a X64 Machine.
        /// Does not require Long Mode.
        /// </summary>
        public const bool UseExecutionProtection = true && UsePAE;

        /// <summary>
        /// If true, there's only a one Process with single Thread.
        /// </summary>
        public const bool SingleThread = false;

        /// <summary>
        /// If true, there's only a one Process with single Thread.
        /// </summary>
        public const bool UseTaskStateSegment = true;

        /// <summary>
        /// If false, no User Segments will be created.
        /// </summary>
        public const bool UseUserMode = true;

        /// <summary>
        /// If enabled, Tasks in UserMode will run with Kernel Permissions
        /// </summary>
        public const bool UserUserModeWithSupervisorPrivileges = false;

        /// <summary>
        /// If enabled, User Task will be able to use IOPorts
        /// </summary>
        public const bool AllowUserModeIOPort = true;

        /// <summary>
        /// Too small value will result in an stackoverflow, a too big value will waste memory.
        /// FUTURE: Request Virtual Memory
        /// </summary>
        public const uint DefaultStackSize = 0x4000;

        public static class Log
        {

            /// <summary>
            /// Trace every Task switch
            /// </summary>
            public const bool TaskSwitch = false;

            /// <summary>
            /// Trace every Thread Action
            /// </summary>
            public const KLogLevel Threads = KLogLevel.Info;

            /// <summary>
            /// Trace every Page Allocation
            /// </summary>
            public const bool PageAllocation = false;

            /// <summary>
            /// Trace every Thread switch
            /// </summary>
            public const bool MemoryMapping = false;

            /// <summary>
            /// Trace every Syscall
            /// </summary>
            public const bool SysCall = false;

            /// <summary>
            /// Trace every Interrupt except Clock
            /// </summary>
            public const bool Interrupts = false;

            /// <summary>
            /// Trace ELF format related calls
            /// </summary>
            public const bool ELF = false;

        }
    }

    public enum KConfigCpu
    {
        x86,
        x64,
    }

}
