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

    public class FrameBuffer : IFrameBuffer
    {

        private Addr _Addr;
        private int _Width;
        private int _Height;
        private int _Pitch;
        private int _Depth;

        public Addr Addr => _Addr;
        public int Width => _Width;
        public int Height => _Height;
        public int Pitch => _Pitch;
        public int Depth => _Depth;

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
            var size = _Height * _Pitch;
            // TODO: Don't replace Addr field
            _Addr = SysCalls.GetPhysicalMemory(_Addr, (uint)size);
        }

        public FrameBuffer(Addr addr, int width, int height, int pitch, int depth)
        {
            this._Addr = addr;
            this._Width = width;
            this._Height = height;
            this._Pitch = pitch;
            this._Depth = depth;
        }

        public void Init()
        {
            var memorySize = (uint)(_Pitch * _Height * 4);
            _Addr = SysCalls.GetPhysicalMemory(_Addr, memorySize);
        }

        public int GetOffset(int x, int y)
        {
            return (y * _Pitch / 4) + x; //4 -> 32bpp
        }

        protected int GetByteOffset(int x, int y)
        {
            return (y * _Pitch) + (x * 4); //4 -> 32bpp
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

            //memory.Write8(GetOffset(x, y), (byte)color);
            ((uint*)_Addr)[GetOffset(x, y)] = nativeColor;

            /*KernelMessage.WriteLine("DEBUG: {0:X9}", GetOffset(x, y));
            KernelMessage.WriteLine("DEBUG: {0:X9}", GetByteOffset(x, y));
            KernelMessage.WriteLine("DEBUG2: {0:D}", color);
            KernelMessage.WriteLine("DEBUG3: {0:X9}", (uint)addr);
*/
        }

    }
}
