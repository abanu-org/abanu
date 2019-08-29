using System;
using Mosa.Runtime;
using Mosa.Runtime.x86;

using System.Runtime.InteropServices;

namespace lonos.Kernel.Core.PageManagement
{
    /// <summary>
    /// Page Table
    /// </summary>
    public unsafe static class PageTable
    {

        public enum PageTableType
        {
            x86,
            PAE
        }

        public static PageTableType Type;

        public static void ConfigureType(PageTableType type) => Type = type;

        public static USize InitalMemoryAllocationSize
        {
            get
            {
                if (Type == PageTableType.x86)
                    return PageTableX86.InitalMemoryAllocationSize;
                else
                    return PageTablePAE.InitalMemoryAllocationSize;
            }
        }

        public static void Setup(Addr entriesAddr)
        {
            if (Type == PageTableType.x86)
                PageTableX86.Setup(entriesAddr);
            else
                PageTablePAE.Setup(entriesAddr);
        }

        public static void KernelSetup(Addr entriesAddr, PageTableType type)
        {
            Type = type;
            if (type == PageTableType.x86)
                PageTableX86.KernelSetup(entriesAddr);
            else
                PageTablePAE.KernelSetup(entriesAddr);
        }

        public static void MapVirtualAddressToPhysical(Addr virtualAddress, Addr physicalAddress, bool present = true)
        {
            if (Type == PageTableType.x86)
                PageTableX86.MapVirtualAddressToPhysical(virtualAddress, physicalAddress, present);
            else
                PageTablePAE.MapVirtualAddressToPhysical(virtualAddress, physicalAddress, present);
        }

        public static void EnableKernelWriteProtection()
        {
            // Set CR0.WP
            Native.SetCR0(Native.GetCR0() | 0x10000);
        }

        public static void DisableKernelWriteProtection()
        {
            // Set CR0.WP
            Native.SetCR0((uint)(Native.GetCR0() & ~0x10000));
        }

        public static void EnableExecutionProtection()
        {
            if (Type == PageTableType.PAE)
                PageTablePAE.EnableExecutionProtection();
        }

        public static void SetKernelWriteProtectionForAllInitialPages()
        {
            if (Type == PageTableType.x86)
                PageTableX86.SetKernelWriteProtectionForAllInitialPages();
            else
                PageTablePAE.SetKernelWriteProtectionForAllInitialPages();
        }

        public static void SetExecutionProtectionForAllInitialPages(LinkedMemoryRegion* currentTextSection)
        {
            if (Type == PageTableType.PAE)
                PageTablePAE.SetExecutionProtectionForAllInitialPages(currentTextSection);
        }

        public static void Flush()
        {
            if (Type == PageTableType.x86)
                PageTableX86.Flush();
            else
                PageTablePAE.Flush();
        }

        public static void Flush(Addr virtAddr)
        {
            if (Type == PageTableType.x86)
                PageTableX86.Flush(virtAddr);
            else
                PageTablePAE.Flush(virtAddr);
        }

        public static void SetKernelWriteProtectionForRegion(uint virtAddr, uint size)
        {
            if (Type == PageTableType.x86)
                PageTableX86.SetKernelWriteProtectionForRegion(virtAddr, size);
            else
                PageTablePAE.SetKernelWriteProtectionForRegion(virtAddr, size);
        }

        public static void SetExecutableForRegion(uint virtAddr, uint size)
        {
            if (Type == PageTableType.PAE)
                PageTablePAE.SetExecutableForRegion(virtAddr, size);
        }

    }

}
