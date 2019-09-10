using System;

namespace Lonos.Kernel.Core.Boot
{
    public struct BootInfoMemory
    {
        public Addr Start;
        public USize Size;
        public BootInfoMemoryType Type;
        //public bool CanWrite;
        //public bool CanExecute;
    }
}
