// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Abanu.Kernel
{
    public class RuntimeElfSectionCollection : List<RuntimeElfSection>
    {
        internal RuntimeElfSectionCollection(int sectionCount)
            : base(sectionCount)
        {
        }

        public RuntimeElfSection this[string name]
        {
            get
            {
                for (var i = 0; i < Count; i++)
                    if (this[i].Name == name)
                        return this[i];

                throw new Exception("Section " + name + " not found");
            }
        }
    }
}
