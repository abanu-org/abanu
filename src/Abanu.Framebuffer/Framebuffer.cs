// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abanu.Runtime;

namespace Abanu.Kernel
{

    public class FrameBuffer : IFrameBuffer
    {

        private Addr _Addr;
        private int _Width;
        private int _Height;
        private int _Pitch;
        private int _Depth;
        private int _MemorySize;

        public Addr Addr => _Addr;
        public int Width => _Width;
        public int Height => _Height;
        public int Pitch => _Pitch;
        public int Depth => _Depth;

        public int MemorySize => _Height * _Pitch;

        public FrameBuffer(ref BootInfoFramebufferInfo info)
        {
            this._Addr = info.FbAddr;
            this._Width = (int)info.FbWidth;
            this._Height = (int)info.FbHeight;
            this._Pitch = (int)info.FbPitch;
            this._Depth = info.FbBpp;
            _MemorySize = (int)info.RequiredMemory;
        }

        public int GetOffset(int x, int y)
        {
            return (y * _Pitch / 4) + x; //4 -> 32bpp
        }

        public unsafe uint GetPixel(int x, int y)
        {
            //return memory.Read8(GetOffset(x, y));
            return ((uint*)_Addr)[GetOffset(x, y)];
        }

        public unsafe void SetPixel(int x, int y, uint nativeColor)
        {
            if (x >= _Width || y >= _Height)
                return;

            ((uint*)_Addr)[GetOffset(x, y)] = nativeColor;
        }

        public unsafe void FillRectangle(int x, int y, int w, int h, uint color)
        {
            for (int offsetY = 0; offsetY < h; offsetY++)
            {
                int startOffset = GetOffset(x, offsetY + y);
                for (uint offsetX = 0; offsetX < w; offsetX++)
                {
                    ((uint*)_Addr)[startOffset + offsetX] = color;
                }
            }
        }

    }
}
