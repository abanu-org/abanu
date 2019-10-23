// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lonos.Runtime;

namespace Lonos.Kernel
{

    public class FrameBuffer
    {

        private Addr Addr;
        public int Width;
        public int Height;
        private int Pitch;
        private int Depth;

        public static unsafe FrameBuffer Create()
        {
            var targetProcId = SysCalls.GetProcessIDForCommand(SysCallTarget.GetFramebufferInfo);
            var fbInfoMem = SysCalls.RequestMessageBuffer(4096, targetProcId);
            SysCalls.GetFramebufferInfo(fbInfoMem);
            var fbPresent = (int*)fbInfoMem.Start;
            if (*fbPresent == 0)
                return null;

            var fbInfo = (BootInfoFramebufferInfo*)(fbInfoMem.Start + 4);
            var fb = new FrameBuffer(fbInfo->FbAddr, (int)fbInfo->FbWidth, (int)fbInfo->FbHeight, (int)fbInfo->FbPitch, (int)fbInfo->FbBpp);
            fb.RequestMemory();
            return fb;
        }

        private void RequestMemory()
        {
            var size = Height * Pitch;
            // TODO: Don't replace Addr field
            Addr = SysCalls.GetPhysicalMemory(Addr, (uint)size);
        }

        public FrameBuffer(Addr addr, int width, int height, int pitch, int depth)
        {
            this.Addr = addr;
            this.Width = width;
            this.Height = height;
            this.Pitch = pitch;
            this.Depth = depth;
        }

        public void Init()
        {
            var memorySize = (uint)(Pitch * Height * 4);
            Addr = SysCalls.GetPhysicalMemory(Addr, memorySize);
        }

        protected int GetOffset(int x, int y)
        {
            return (y * Pitch / 4) + x; //4 -> 32bpp
        }

        protected int GetByteOffset(int x, int y)
        {
            return (y * Pitch) + (x * 4); //4 -> 32bpp
        }

        public unsafe uint GetPixel(int x, int y)
        {
            //return memory.Read8(GetOffset(x, y));
            return ((uint*)Addr)[GetOffset(x, y)];
        }

        public unsafe void SetPixel(int color, int x, int y)
        {
            if (x >= Width || y >= Height)
                return;

            //memory.Write8(GetOffset(x, y), (byte)color);
            ((uint*)Addr)[GetOffset(x, y)] = (uint)color;

            /*KernelMessage.WriteLine("DEBUG: {0:X9}", GetOffset(x, y));
            KernelMessage.WriteLine("DEBUG: {0:X9}", GetByteOffset(x, y));
            KernelMessage.WriteLine("DEBUG2: {0:D}", color);
            KernelMessage.WriteLine("DEBUG3: {0:X9}", (uint)addr);
*/
        }

        public unsafe void FillRectangle(int color, int x, int y, int w, int h)
        {
            for (int offsetY = 0; offsetY < h; offsetY++)
            {
                int startOffset = GetOffset(x, offsetY + y);
                for (int offsetX = 0; offsetX < w; offsetX++)
                {
                    //memory.Write8(startAddress + offsetX, (byte)color);
                    ((uint*)Addr)[startOffset + offsetX] = (uint)color;
                }
            }
        }

    }
}
