using System;
namespace lonos.kernel.core
{

    [Flags]
    public enum PageStatus : uint
    {
        Unset = 0,
        Free = 1,
        Used = 2,
        Reserved = 4
    }

    public unsafe struct Page
    {
        public Addr PhysicalAddress;
        public Page* Next;
        public PageStatus Status;

        public USize Size => 4096;

        public bool Free => ((uint)Status).IsMaskSet((uint)PageStatus.Free);
        public bool Unset => ((uint)Status).IsMaskSet((uint)PageStatus.Unset);
        public bool Used => ((uint)Status).IsMaskSet((uint)PageStatus.Used);
        public bool Reserved => ((uint)Status).IsMaskSet((uint)PageStatus.Reserved);
    }
}
