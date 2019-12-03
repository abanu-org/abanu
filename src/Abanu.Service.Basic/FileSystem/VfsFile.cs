// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System.IO;

namespace Abanu.Kernel
{

    internal class VfsFile
    {
        // TODO: Shared. Consider converting to Block-Device.
        public Stream Buffer;

        public string Path;
        public int Length;
    }

}
