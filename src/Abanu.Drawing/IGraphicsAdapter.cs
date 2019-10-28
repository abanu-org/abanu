// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

namespace Abanu.Kernel
{
    public interface IGraphicsAdapter
    {
        void SetPixel(int x, int y, uint nativeColor);
        void FillRectangle(int x, int y, int w, int h, uint nativeColor);
        void Flush();

        ISurface Target { get; set; }

    }
}
