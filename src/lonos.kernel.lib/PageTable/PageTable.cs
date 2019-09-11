// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Runtime.InteropServices;
using Mosa.Runtime;
using Mosa.Runtime.x86;

namespace Lonos.Kernel.Core.PageManagement
{
    /// <summary>
    /// Page Table
    /// </summary>
    public unsafe abstract partial class PageTable : IPageTable
    {

        public static IPageTable KernelTable;

        public static void ConfigureType(PageTableType type)
        {

            KernelTable = CreateInstance(type);
        }

        public static IPageTable CreateInstance()
        {
            return CreateInstance(KernelTable.Type);
        }

        private static IPageTable CreateInstance(PageTableType type)
        {
            if (type == PageTableType.x86)
                return new PageTableX86();
            else
                return new PageTablePAE();
        }

        public abstract PageTableType Type { get; }

        public abstract USize InitalMemoryAllocationSize { get; }

        public abstract Addr VirtAddr { get; }

        public abstract void Setup(Addr entriesAddr);

        public abstract void KernelSetup(Addr entriesAddr);

        public abstract void UserProcSetup(Addr entriesAddr);

        public abstract void MapVirtualAddressToPhysical(Addr virtualAddress, Addr physicalAddress, bool present = true);

        public void EnableKernelWriteProtection()
        {
            // Set CR0.WP
            Native.SetCR0(Native.GetCR0() | 0x10000);
        }

        public void DisableKernelWriteProtection()
        {
            // Set CR0.WP
            Native.SetCR0((uint)(Native.GetCR0() & ~0x10000));
        }

        public virtual void EnableExecutionProtection() { }

        public abstract void SetKernelWriteProtectionForAllInitialPages();

        public virtual void SetExecutionProtectionForAllInitialPages(LinkedMemoryRegion* currentTextSection)
        {
        }

        public abstract void Flush();

        public abstract void Flush(Addr virtAddr);

        public abstract void SetWritable(uint virtAddr, uint size);

        public virtual void SetExecutable(uint virtAddr, uint size) { }

        public abstract Addr GetPhysicalAddressFromVirtual(Addr virtualAddress);
    }

}
