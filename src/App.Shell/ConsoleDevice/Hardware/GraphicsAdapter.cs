// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

namespace Lonos.Kernel
{
    public class GraphicsAdapter : IGraphicsAdapter
    {
        private Addr _Addr;
        private int _Width;
        private int _Height;
        private int _Depth;
        private int _Pitch;

        private ISurface _Target;
        public ISurface Target
        {
            get => _Target;
            set
            {
                _Target = value;
                _Target = value;
                _Addr = value.Addr;
                _Width = value.Width;
                _Height = value.Height;
                _Depth = value.Depth;
                _Pitch = value.Pitch;
            }
        }

        public GraphicsAdapter(ISurface dev)
        {
            Target = dev;
        }

        public void SetPixel(int x, int y, uint nativeColor)
        {
            _Target.SetPixel(x, y, nativeColor);
        }

        public unsafe void FillRectangle(int x, int y, int w, int h, uint nativeColor)
        {
            uint addr = _Target.Addr;
            for (int offsetY = 0; offsetY < h; offsetY++)
            {
                int startOffset = _Target.GetOffset(x, offsetY + y);
                for (int offsetX = 0; offsetX < w; offsetX++)
                {
                    ((uint*)addr)[startOffset + offsetX] = nativeColor;
                }
            }
        }

        public void Flush()
        {
        }

    }
}
