// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Threading
{

    public class LocalDataStoreSlot
    {
        internal int Position;

        internal LocalDataStoreSlot(int pos)
        {
            Position = pos;
        }

    }

}
