using lonos.Kernel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lonos.Kernel.Core.PageManagement
{

    public static class PageTableExtensions
    {
        public static void Map(this IPageTable table, Addr virtAddr, Addr physAddr, USize length, bool present = true, bool flush = false)
        {
            KernelMessage.WriteLine("Map: virt={0:X8}, phys={0:X8}, length={0:X8}", virtAddr, physAddr, length);
            var pages = KMath.DivCeil(length, 4096);
            for (var i = 0; i < pages; i++)
            {
                table.MapVirtualAddressToPhysical(virtAddr, physAddr, present);

                virtAddr += 4096;
                physAddr += 4096;
            }
            if (flush)
                table.Flush();
        }
    }

    public unsafe interface IPageTable
    {
        PageTableType Type { get; }

        USize InitalMemoryAllocationSize { get; }

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

    }

}
