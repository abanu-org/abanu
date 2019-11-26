// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System.Runtime.InteropServices;

namespace Abanu.Kernel
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Stat
    {
        public int Dev;
        public ulong Ino;
        public int Mode;
        public int Nlink;
        public int Uid;
        public int Gid;
        public int Rdev;
        public int Size;
        public int Blksize;
        public int Blocks;
        public int Atime;
        public int Mtime;
        public int Ctime;
    }

}
