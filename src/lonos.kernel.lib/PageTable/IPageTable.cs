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
        Addr VirtAddr { get; }

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

        void SetWritable(uint virtAddr, uint size);

        void SetExecutable(uint virtAddr, uint size);

        Addr GetPhysicalAddressFromVirtual(Addr virtualAddress);

    }

}
