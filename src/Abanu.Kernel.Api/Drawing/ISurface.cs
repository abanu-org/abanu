// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

namespace Abanu.Kernel
{

    public interface ISurface
    {
        Addr Addr { get; }
        int Width { get; }
        int Height { get; }
        int Pitch { get; }
        int Depth { get; }

        SurfaceDeviceType DeviceType { get; }

        uint GetPixel(int x, int y);
        void SetPixel(int x, int y, uint nativeColor);

        int GetOffset(int x, int y);
        void Flush();
    }
}
