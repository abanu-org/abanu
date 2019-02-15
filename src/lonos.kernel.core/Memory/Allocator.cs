using System;
namespace lonos.kernel.core
{

    public unsafe struct AllocBlockHeader
    {
        public USize Size;
        //public USize Free;

        public AllocBlockHeader* Next;
        public AllocBlockHeader* Prev;

        public AllocHeader* Head;

        public bool IsPageAlloc => sizeof(AllocBlockHeader) + Size >= 4096;

        public uint FreeHeadSize
        {
            get
            {
                if (IsPageAlloc)
                    return 0;
                fixed (AllocBlockHeader* ptr = &this)
                {
                    var addr = (Addr)ptr + sizeof(AllocBlockHeader);
                    var headAddr = (Addr)Head;
                    return headAddr - addr;
                }
            }
        }
    }

    public unsafe struct AllocHeader
    {
        public AllocHeader* Next;
        public AllocHeader* Prev;
        public USize Size;

        public AllocBlockHeader* PageHead
        {
            get
            {
                fixed (AllocHeader* ptr = &this)
                {
                    var addr = (Addr)ptr;
                    var startAddr = KMath.AlignValueFloor(addr, 4096);
                    return (AllocBlockHeader*)startAddr;
                }
            }
        }

        public Addr Data
        {
            get
            {
                fixed (AllocHeader* ptr = &this)
                {
                    return ptr + sizeof(AllocHeader);
                }
            }
        }

        public uint FreeTailSize
        {
            get
            {
                fixed (AllocHeader* ptr = &this)
                {
                    var addr = (Addr)ptr;
                    var nextAddr = KMath.AlignValueCeil(addr, 4096);

                    if (Next != null)
                        nextAddr = (Addr)Next;
                    return nextAddr - addr;
                }
            }
        }

    }

    public unsafe class Allocator
    {


        public Allocator()
        {
        }

        private AllocBlockHeader* PageTail;
        private AllocBlockHeader* PageHead;

        private AllocBlockHeader* Tail;
        private AllocBlockHeader* Head;

        public Addr Allocate(USize size)
        {

            uint allocBlockSize = (uint)sizeof(AllocBlockHeader) + (uint)sizeof(AllocHeader) + size;

            if (allocBlockSize >= 4096)
            {
                var addr = Memory.RequestRawVirtalMemoryPages(KMath.DivCeil(allocBlockSize, 4096));
                var block = (AllocBlockHeader*)addr;
                var header = (AllocHeader*)(addr + sizeof(AllocBlockHeader));
                header->Next = null;
                header->Size = size;
                block->Size = size + (uint)sizeof(AllocHeader);
                block->Next = null;
                block->Head = header;
                PageTail->Next = block;
                if (PageHead == null)
                    PageHead = block;
                return addr + (uint)sizeof(AllocBlockHeader) + (uint)sizeof(AllocHeader);
            }
            else
            {
                uint allocSize = (uint)sizeof(AllocBlockHeader) + size;

                AllocHeader* foundH = null;
                AllocBlockHeader* foundB = null;
                var b = Head; // TODO: Convert First-Fit to Next-Fit!
                //var wrapped = false; For Next Fit
                while (true)
                {
                    if (Head == null)
                        break;
                    if (b == null)
                        break;

                    if (b->FreeHeadSize >= allocSize)
                    {
                        foundB = b;
                        break;
                    }

                    AllocHeader* h = b->Head;
                    while (true)
                    {
                        if (h == null)
                            break;
                        if (h->FreeTailSize > allocSize)
                        {
                            foundH = h;
                            foundB = b;
                            break;
                        }
                        h = h->Next;
                    }

                    if (foundH != null)
                        break;
                }


                if (foundB == null) // no block found. So, no Header also
                {
                    foundB = (AllocBlockHeader*)Memory.RequestRawVirtalMemoryPages(1);
                    foundB->Size = 0;
                    foundB->Next = null;
                    if (Head == null)
                        Head = foundB;
                    Tail = foundB;

                    foundH = (AllocHeader*)(((Addr)foundB) + (uint)sizeof(AllocHeader));
                    foundH->Size = size;
                    foundH->Next = null;
                    foundB->Head = foundH;
                    return foundH->Data;
                }
                else
                {
                    if (foundH == null)
                    { // found block, aber no header: Insert ab begin
                        foundH = (AllocHeader*)(((Addr)foundB) + (uint)sizeof(AllocHeader));
                        foundH->Size = size;
                        foundH->Next = foundB->Head;
                        foundB->Head = foundH;
                        return foundH->Data;
                    }
                    else
                    { // found block, and header: append
                        var newH = (AllocHeader*)(((Addr)foundH) + (uint)sizeof(AllocHeader) + foundH->Size);
                        newH->Size = size;
                        newH->Next = foundH->Next;
                        foundH->Next = newH;
                        return newH->Data;
                    }
                }
            }
        }

        public void Free(Addr addr)
        {
            var header = (AllocHeader*)(addr - (uint)sizeof(AllocHeader));
            uint allocBlockSize = (uint)sizeof(AllocBlockHeader) + (uint)sizeof(AllocHeader) + header->Size;
            if (allocBlockSize >= 4096)
            {
                var block = (AllocBlockHeader*)(addr - (uint)sizeof(AllocBlockHeader) - (uint)sizeof(AllocBlockHeader));

                Memory.FreeRawVirtalMemoryPages(header);
                return;
            }
            else
            {
            }
        }

    }

}
