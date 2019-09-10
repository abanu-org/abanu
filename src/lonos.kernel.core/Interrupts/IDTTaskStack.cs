// Copyright (c) MOSA Project. Licensed under the New BSD License.

using System.Runtime.InteropServices;

namespace lonos.Kernel.Core.Interrupts
{
    /// <summary>
    /// IDT Stack
    /// </summary>

    [StructLayout(LayoutKind.Explicit)]
    public struct IDTTaskStack
    {
        public const int Size = 60;

        [FieldOffset(0)]
        public IDTStack Stack;

        [FieldOffset(IDTStack.Offset.TASK_ESP)]
        public uint TASK_ESP;

        [FieldOffset(IDTStack.Offset.TASK_SS)]
        public uint TASK_SS;
    }
}
