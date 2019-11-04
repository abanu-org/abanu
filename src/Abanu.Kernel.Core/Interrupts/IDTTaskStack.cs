// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System.Runtime.InteropServices;

namespace Abanu.Kernel.Core.Interrupts
{
    /// <summary>
    /// If a user space process is interrupted, we have additional values on stack
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
