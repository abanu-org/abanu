// Copyright (c) MOSA Project. Licensed under the New BSD License.

using lonos.Kernel.Core.PageManagement;

namespace lonos.Kernel.Core.Devices
{

    public class FrameBuffer
    {

        private Addr addr;
        public uint width;
        public uint height;
        private uint pitch;
        private uint depth;

        public FrameBuffer(Addr addr, uint width, uint height, uint pitch, uint depth)
        {
            this.addr = addr;
            this.width = width;
            this.height = height;
            this.pitch = pitch;
            this.depth = depth;
        }

        public void Init()
        {
            uint memorySize = (uint)(pitch * height * 4);
            RequestPhysicalMemory(addr, memorySize);
        }

        private static void RequestPhysicalMemory(uint address, uint size)
        {
            // Map physical memory space to virtual memory space
            for (uint at = address; at < address + size; at += 4096)
            {
                PageTable.KernelTable.MapVirtualAddressToPhysical(at, at);
            }
            PageTable.KernelTable.Flush();

            //return new Memory(new IntPtr(address), size);
        }

        protected uint GetOffset(uint x, uint y)
        {
            return (y * pitch / 4) + x; //4 -> 32bpp
        }

        protected uint GetByteOffset(uint x, uint y)
        {
            return (y * pitch) + (x * 4); //4 -> 32bpp
        }

        public unsafe uint GetPixel(uint x, uint y)
        {
            //return memory.Read8(GetOffset(x, y));
            return ((uint*)addr)[GetOffset(x, y)];
        }

        public unsafe void SetPixel(uint color, uint x, uint y)
        {
            if (x >= width || y >= height)
                return;

            //memory.Write8(GetOffset(x, y), (byte)color);
            ((uint*)addr)[GetOffset(x, y)] = (uint)color;

            /*KernelMessage.WriteLine("DEBUG: {0:X9}", GetOffset(x, y));
            KernelMessage.WriteLine("DEBUG: {0:X9}", GetByteOffset(x, y));
            KernelMessage.WriteLine("DEBUG2: {0:D}", color);
            KernelMessage.WriteLine("DEBUG3: {0:X9}", (uint)addr);
*/
        }

        public unsafe void FillRectangle(uint color, uint x, uint y, uint w, uint h)
        {
            for (uint offsetY = 0; offsetY < h; offsetY++)
            {
                uint startAddress = GetOffset(x, offsetY + y);
                for (uint offsetX = 0; offsetX < w; offsetX++)
                {
                    //memory.Write8(startAddress + offsetX, (byte)color);
                    ((uint*)addr)[startAddress + offsetX] = (uint)color;
                }
            }
        }

    }
}
