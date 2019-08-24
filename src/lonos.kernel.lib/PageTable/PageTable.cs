using System;
using Mosa.Runtime;
using Mosa.Runtime.x86;

using System.Runtime.InteropServices;

namespace lonos.kernel.core
{
    /// <summary>
    /// Page Table
    /// </summary>
    public unsafe static class PageTable
    {

        public enum PageTableType
        {
            x86,
            x64
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
                    return PageTableX64.InitalMemoryAllocationSize;
            }
        }

        public static void Setup(Addr entriesAddr)
        {
            if (Type == PageTableType.x86)
                PageTableX86.Setup(entriesAddr);
            else
                PageTableX64.Setup(entriesAddr);
        }

        public static void KernelSetup(Addr entriesAddr, PageTableType type)
        {
            Type = type;
            if (type == PageTableType.x86)
                PageTableX86.KernelSetup(entriesAddr);
            else
                PageTableX64.KernelSetup(entriesAddr);
        }

        public static void MapVirtualAddressToPhysical(Addr virtualAddress, Addr physicalAddress, bool present = true)
        {
            if (Type == PageTableType.x86)
                PageTableX86.MapVirtualAddressToPhysical(virtualAddress, physicalAddress, present);
            else
                PageTableX64.MapVirtualAddressToPhysical(virtualAddress, physicalAddress, present);
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
            if (Type == PageTableType.x64)
                PageTableX64.EnableExecutionProtection();
        }

        public static void SetKernelWriteProtectionForAllInitialPages()
        {
            if (Type == PageTableType.x86)
                PageTableX86.SetKernelWriteProtectionForAllInitialPages();
            else
                PageTableX64.SetKernelWriteProtectionForAllInitialPages();
        }

        public static void SetExecutionProtectionForAllInitialPages(LinkedMemoryRegion* currentTextSection)
        {
            if (Type == PageTableType.x64)
                PageTableX64.SetExecutionProtectionForAllInitialPages(currentTextSection);
        }

        public static void Flush()
        {
            if (Type == PageTableType.x86)
                PageTableX86.Flush();
            else
                PageTableX64.Flush();
        }

        public static void Flush(Addr virtAddr)
        {
            if (Type == PageTableType.x86)
                PageTableX86.Flush(virtAddr);
            else
                PageTableX64.Flush(virtAddr);
        }

        public static void SetKernelWriteProtectionForRegion(uint virtAddr, uint size)
        {
            if (Type == PageTableType.x86)
                PageTableX86.SetKernelWriteProtectionForRegion(virtAddr, size);
            else
                PageTableX64.SetKernelWriteProtectionForRegion(virtAddr, size);
        }

        public static void SetExecutableForRegion(uint virtAddr, uint size)
        {
            if (Type == PageTableType.x64)
                PageTableX64.SetExecutableForRegion(virtAddr, size);
        }

    }

}
