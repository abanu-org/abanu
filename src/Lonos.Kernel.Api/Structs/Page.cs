// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Runtime.InteropServices;
using Lonos.CTypes;

namespace Lonos.Kernel.Core
{

    [Flags]
    public enum PageStatus : uint
    {
        Unset = 0,
        Free = 1,
        Used = 2,
        Reserved = 4,
    }

    // This struct can be used for both phys and virt page management. Do not add Specializations!

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public unsafe struct Page
    {

        // Fields for buddy allocator
        public list_head Lru;
        public uint Flags;
        //union {
        public byte Order;
        public Page* FirstPage;
        //};

        //---

        public PageStatus Status;
        public Atomic UsageCount;

        public Addr Address;

        // If this is the head of an allocated block
        public Page* Head;

        // If this is the tail of an allocated block
        public Page* Tail;

        /// <summary>
        /// Number of reserved Page for this allocation. Only set if this is the Head page.
        /// </summary>
        public uint PagesUsed;

        // TODO: Remove the Next-field, because it could be accessed via (Page*)+1

        public Page* Next;

        public static USize Size => 4096;

        public uint PageNum => Address / 4096;

        public bool Free => ((uint)Status).IsMaskSet((uint)PageStatus.Free);

        public bool Unset => ((uint)Status).IsMaskSet((uint)PageStatus.Unset);

        public bool Used => ((uint)Status).IsMaskSet((uint)PageStatus.Used);

        public bool Reserved => ((uint)Status).IsMaskSet((uint)PageStatus.Reserved);
    }
}
