// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

namespace Lonos.Kernel
{

    public interface IFrameBuffer
    {
        Addr Addr { get; }
        int Width { get; }
        int Height { get; }
        int Pitch { get; }
        int Depth { get; }

        uint GetPixel(int x, int y);
        void SetPixel(int x, int y, uint nativeColor);

        int GetOffset(int x, int y);
    }
}
