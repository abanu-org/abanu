using lonos.Kernel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lonos.Kernel.Core.PageManagement
{

    public unsafe interface IPageTable
    {
        PageTableType Type { get; }

        USize InitalMemoryAllocationSize { get; }
        Addr GdtAddr { get; }

        void Setup(Addr entriesAddr);

        void UserProcSetup(Addr entriesAddr);

        void KernelSetup(Addr entriesAddr);

        void MapVirtualAddressToPhysical(Addr virtualAddress, Addr physicalAddress, bool present = true);

        void EnableKernelWriteProtection();

        void DisableKernelWriteProtection();

        void EnableExecutionProtection();

        void SetKernelWriteProtectionForAllInitialPages();

        void SetExecutionProtectionForAllInitialPages(LinkedMemoryRegion* currentTextSection);

        void Flush();
        void Flush(Addr virtAddr);

        void SetKernelWriteProtectionForRegion(uint virtAddr, uint size);

        void SetExecutableForRegion(uint virtAddr, uint size);

        Addr GetPhysicalAddressFromVirtual(Addr virtualAddress);

    }

}
