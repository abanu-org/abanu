// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

namespace Abanu.Kernel
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

        public GraphicsAdapter(ISurface target)
        {
            Target = target;
        }

        public void SetPixel(int x, int y, uint nativeColor)
        {
            _Target.SetPixel(x, y, nativeColor);
        }

        private unsafe void FillRectangle(int x, int y, int w, int h, uint nativeColor)
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

        private unsafe void FillRectangle(int x, int y, int w, int h, ISurface source, int sourceX, int sourceY)
        {
            // TODO: Range check / correction

            uint addr = _Target.Addr;
            for (int offsetY = 0; offsetY < h; offsetY++)
            {
                int startOffset = _Target.GetOffset(x, offsetY + y);
                for (int offsetX = 0; offsetX < w; offsetX++)
                {
                    var color = source.GetPixel(offsetX + sourceX, offsetY + sourceY);

                    ((uint*)addr)[startOffset + offsetX] = color;
                }
            }
        }

        public void Flush()
        {
            Target.Flush();
        }

        private ISurface _SourceSurface;
        // TODO: Struct
        private int _SourceSurfaceX;
        private int _SourceSurfaceY;
        private uint _SourceColor;

        private void ResetSource()
        {
            _SourceSurface = null;
            _SourceColor = 0;
            _SourceSurfaceX = 0;
            _SourceSurfaceY = 0;
        }

        public void SetSource(uint nativeColor)
        {
            ResetSource();
            _SourceColor = nativeColor;
        }

        public void SetSource(ISurface sourceSurface, int sourceX, int sourceY)
        {
            ResetSource();
            _SourceSurface = sourceSurface;
            _SourceSurfaceX = sourceX;
            _SourceSurfaceY = sourceY;
        }

        // TODO: Struct
        private int RectX;
        private int RectY;
        private int RectW;
        private int RectH;

        public void Rectangle(int x, int y, int w, int h)
        {
            RectX = x;
            RectY = y;
            RectW = w;
            RectH = h;
        }

        public void Fill()
        {
            if (_SourceSurface != null)
                FillRectangle(RectX, RectY, RectW, RectH, _SourceSurface, _SourceSurfaceX, _SourceSurfaceY);
            else
                FillRectangle(RectX, RectY, RectW, RectH, _SourceColor);
        }
    }
}
