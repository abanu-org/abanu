using lonos.Kernel.Core.PageManagement;
using System;
namespace lonos.Kernel.Core.MemoryManagement
{
    public class RawVirtualFrameAllocator
    {

        private static Addr _startVirtAddr;
        private static Addr _nextVirtAddr;

        private static Addr _identityStartVirtAddr;
        private static Addr _identityNextVirtAddr;

        public static void Setup()
        {
            _startVirtAddr = 0x40000000; //1gb
            _nextVirtAddr = _startVirtAddr;

            _identityStartVirtAddr = 0x6400000; //100mb
            _identityNextVirtAddr = _identityStartVirtAddr;

        }

        /// <summary>
        /// Returns raw, unmanaged Memory.
        /// Consumer: Kernel, Memory allocators
        /// Shoud be used for larger Chunks.
        /// </summary>
        internal unsafe static Addr RequestRawVirtalMemoryPages(uint pages)
        {
            Addr virt = _nextVirtAddr;
            var head = PageFrameManager.AllocatePages(PageFrameRequestFlags.Default, pages);
            if (head == null)
                return Addr.Zero;

            var p = head;
            for (var i = 0; i < pages; i++)
            {
                PageTable.KernelTable.MapVirtualAddressToPhysical(_nextVirtAddr, p->PhysicalAddress);
                _nextVirtAddr += 4096;
                p = p->Next;
            }
            PageTable.KernelTable.Flush();
            return virt;
        }

        internal unsafe static Addr RequestIdentityMappedVirtalMemoryPages(uint pages)
        {
            Addr virt = _identityNextVirtAddr;
            var head = PageFrameManager.GetPhysPage(virt);
            if (head == null)
                return Addr.Zero;

            var p = head;
            for (var i = 0; i < pages; i++)
            {
                p->Status = PageStatus.Used;
                PageTable.KernelTable.MapVirtualAddressToPhysical(_identityNextVirtAddr, p->PhysicalAddress);
                _identityNextVirtAddr += 4096;
                p = p->Next;
            }
            PageTable.KernelTable.Flush();
            return virt;
        }

        internal unsafe static void FreeRawVirtalMemoryPages(Addr virtAddr)
        {

        }

    }
}
