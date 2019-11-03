// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;

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

        public void Stroke()
        {
            var currentPosX = 0;
            var currentPosY = 0;
            for (var i = 0; i < Paths.Count; i++)
            {
                var entry = Paths[i];
                switch (entry.Type)
                {
                    case PathEntryType.Line:
                        DrawLine(currentPosX, currentPosY, entry.X, entry.Y);
                        break;
                }
                currentPosX = entry.X;
                currentPosY = entry.Y;
            }
        }

        private void ClearPath()
        {

        }

        private void DrawLine(int x0, int y0, int x1, int y1)
        {
            if (Math.Abs(y1 - y0) < Math.Abs(x1 - x0))
            {
                if (x0 > x1)
                    DrawLineLow(x1, y1, x0, y0);
                else
                    DrawLineLow(x0, y0, x1, y1);
            }
            else
            {
                if (y0 > y1)
                    DrawLineHigh(x1, y1, x0, y0);
                else
                    DrawLineHigh(x0, y0, x1, y1);
            }
        }

        private void DrawLineLow(int x0, int y0, int x1, int y1)
        {
            var dx = x1 - x0;
            var dy = y1 - y0;
            var yi = 1;
            if (dy < 0)
            {
                yi = -1;
                dy = -dy;
            }
            var d = (2 * dy) - dx;
            var y = y0;

            for (var x = x0; x <= x1; x++)
            {
                SetPixel(x, y, _SourceColor);
                if (d > 0)
                {
                    y = y + yi;
                    d = d - (2 * dx);
                }

                d = d + (2 * dy);
            }
        }

        private void DrawLineHigh(int x0, int y0, int x1, int y1)
        {
            var dx = x1 - x0;
            var dy = y1 - y0;
            var xi = 1;
            if (dx < 0)
            {
                xi = -1;
                dx = -dx;
            }
            var d = (2 * dx) - dy;
            var x = x0;

            for (var y = y0; y <= y1; y++)
            {
                SetPixel(x, y, _SourceColor);
                if (d > 0)
                {
                    x = x + xi;
                    d = d - (2 * dy);
                }
                d = d + (2 * dx);
            }
        }

        public void MoveTo(int x, int y)
        {
            Paths.Add(new PathMoveEntry(x, y));
        }

        public void LineTo(int x, int y)
        {
            Paths.Add(new PathLineEntry(x, y));
        }

        private List<PathEntry> Paths = new List<PathEntry>();

        private class PathEntry
        {

            public int X;
            public int Y;

            public PathEntry(PathEntryType type, int x, int y)
            {
                Type = type;
                X = x;
                Y = y;
            }

            public PathEntryType Type;
        }

        private class PathLineEntry : PathEntry
        {
            public PathLineEntry(int x, int y)
                : base(PathEntryType.Line, x, y)
            {
            }
        }

        private class PathMoveEntry : PathEntry
        {
            public PathMoveEntry(int x, int y)
                : base(PathEntryType.Move, x, y)
            {
            }
        }

        private enum PathEntryType
        {
            Move,
            Line,
        }

    }
}
