// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System;

#pragma warning disable CA1052 // Static holder types should be Static or NotInheritable

namespace Abanu.Kernel.Core
{

    public class ApiContext
    {

        public static IKernelApi Current;

    }

}
