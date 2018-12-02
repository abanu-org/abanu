using System;
namespace lonos.kernel.core
{

    public struct Allocator
    {
        public Addr BaseAddr;
        public USize MaxSize;

        private Addr _current;

        public Allocator(Addr baseAddr, USize maxSize)
        {
            BaseAddr = baseAddr;
            MaxSize = maxSize;
            _current = baseAddr;
        }

        public Addr Allocate(USize size)
        {
            var ret = _current;
            _current += size;
            return ret;
        }

        public void Free(Addr addr)
        {
        }

    }

}
