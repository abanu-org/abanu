// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

namespace Abanu.Kernel
{
    public class MemorySurface : ISurface
    {
        private Addr _Addr;
        private int _Width;
        private int _Height;
        private int _Depth;
        private int _Pitch;

        public Addr Addr => _Addr;
        public int Width => _Width;
        public int Height => _Height;
        public int Depth => _Depth;
        public int Pitch => _Pitch;

        public SurfaceDeviceType DeviceType => SurfaceDeviceType.Framebuffer;

        public MemorySurface(Addr addr, int width, int height, int pitch, int depth)
        {
            _Addr = addr;
            _Width = width;
            _Height = height;
            _Pitch = pitch;
            _Depth = depth;
        }

        public unsafe uint GetPixel(int x, int y)
        {
            return ((uint*)_Addr)[GetOffset(x, y)];
        }

        public unsafe void SetPixel(int x, int y, uint nativeColor)
        {
            if (x >= _Width || y >= _Height)
                return;

            ((uint*)_Addr)[GetOffset(x, y)] = nativeColor;
        }

        public int GetOffset(int x, int y)
        {
            return (y * _Pitch / 4) + x; //4 -> 32bpp
        }

        public void Flush()
        {
        }
    }
}
