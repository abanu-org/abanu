// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Runtime.InteropServices;
using Lonos.CTypes;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable SA1400 // Access modifier should be declared
#pragma warning disable SA1307 // Accessible fields should begin with upper-case letter
#pragma warning disable CA1822 // Mark members as static
#pragma warning disable SA1025 // Code should not contain multiple whitespace in a row
#pragma warning disable SA1502 // Element should not be on a single line
#pragma warning disable SA1119 // Statement should not use unnecessary parenthesis
#pragma warning disable SA1120 // Comments should contain text
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
#pragma warning disable SA1117 // Parameters should be on same line or separate lines
#pragma warning disable SA1116 // Split parameters should start on line after declaration

namespace Lonos.Kernel.Core
{

    [Flags]
    public enum PageStatus : uint
    {
        Unset = 0,
        Free = 1,
        Used = 2,
        Reserved = 4,
        Debug = 5,
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

        // not needed by buddy allocator, but for other allocators
        public Addr Address;

        public static USize Size => 4096;

        //public uint PageNum => Address / 4096;

        //public bool Free => ((uint)Status).IsMaskSet((uint)PageStatus.Free);

        //public bool Unset => ((uint)Status).IsMaskSet((uint)PageStatus.Unset);

        //public bool Used => ((uint)Status).IsMaskSet((uint)PageStatus.Used);

        //public bool Reserved => ((uint)Status).IsMaskSet((uint)PageStatus.Reserved);
    }
}
