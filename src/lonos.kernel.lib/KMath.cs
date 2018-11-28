﻿using System;
using lonos.kernel.core;

namespace lonos.kernel
{
    public static class KMath
    {

        public static uint DivCeil(uint value, uint dividor)
        {
            return (value - 1) / dividor + 1;
        }

        public static ulong DivCeil(ulong value, ulong dividor)
        {
            return (value - 1) / dividor + 1;
        }

        public static USize DivCeil(USize value, USize dividor)
        {
            return (value - 1) / dividor + 1;
        }

    }
}