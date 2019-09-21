// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using Lonos.Kernel.Core;

namespace Lonos
{

    public static class KMath
    {

        public static uint AlignValueCeil(uint value, uint dividor)
        {
            return (value / dividor * dividor) + dividor;
        }

        public static uint AlignValueFloor(uint value, uint dividor)
        {
            return value / dividor * dividor;
        }

        public static uint DivCeil(uint value, uint dividor)
        {
            return ((value - 1) / dividor) + 1;
        }

        public static ulong DivCeil(ulong value, ulong dividor)
        {
            return ((value - 1) / dividor) + 1;
        }

        public static USize DivCeil(USize value, USize dividor)
        {
            return ((value - 1) / dividor) + 1;
        }

        public static uint DivFloor(uint value, uint dividor)
        {
            return value / dividor;
        }

    }
}
