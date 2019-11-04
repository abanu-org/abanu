// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

namespace Abanu.Kernel
{
    public class FramebufferSurface : ISurface
    {
        private Addr _addr;
        private int _Width;
        private int _Height;
        private int _Depth;
        private int _Pitch;

        public Addr Addr => _addr;
        public int Width => _Width;
        public int Height => _Height;
        public int Depth => _Depth;
        public int Pitch => _Pitch;

        public SurfaceDeviceType DeviceType => SurfaceDeviceType.Framebuffer;

        private IFrameBuffer Dev;

        public FramebufferSurface(IFrameBuffer dev)
        {
            Dev = dev;
            _addr = dev.Addr;
            _Width = dev.Width;
            _Height = dev.Height;
            _Depth = dev.Depth;
            _Pitch = dev.Pitch;
        }

        public uint GetPixel(int x, int y)
        {
            return Dev.GetPixel(x, y);
        }

        public void SetPixel(int x, int y, uint nativeColor)
        {
            Dev.SetPixel(x, y, nativeColor);
        }

        public int GetOffset(int x, int y)
        {
            return Dev.GetOffset(x, y);
        }

        public void Flush()
        {
        }
    }
}
