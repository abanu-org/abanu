using lonos.Kernel.Core.PageManagement;
using System;
namespace lonos.Kernel.Core.MemoryManagement
{
    public class RawVirtualFrameAllocator
    {

        private static Addr _startVirtAddr;
        private static Addr _nextVirtAddr;

        public static void Setup()
        {
            _startVirtAddr = 0x40000000; //1gb
            _nextVirtAddr = _startVirtAddr;
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

        internal unsafe static void FreeRawVirtalMemoryPages(Addr virtAddr)
        {

        }

    }
}
