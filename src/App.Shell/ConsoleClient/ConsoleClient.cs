// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Lonos;
using Lonos.Kernel.Core;
using Lonos.Runtime;

namespace Lonos.Kernel
{

    public class ConsoleClient
    {

        private IBuffer file;

        public void Init()
        {
            file = new ConsoleServer();
        }

        private void SendCommand()
        {

        }

        private void SendCommand(string command)
        {
            SendByte(ConsoleServerConstants.ESC);
            SendByte('[');
            SendBytes(command);
        }

        private void SendByte(byte data)
        {

        }

        private void SendByte(char data)
        {
            file.Write(data);
        }

        private void SendBytes(string data)
        {
            file.Write(data);
        }

        public void Clear()
        {
            Write("\x001B[2J");
        }

        public void Reset()
        {
            Write("\x001B[c");
        }

        public void Write(string msg)
        {
            SendBytes(msg);
        }

        public unsafe void WriteLine(NullTerminatedString* str)
        {
            Write(str);
            Write('\n');
        }

        public unsafe void Write(NullTerminatedString* str)
        {
            var len = str->GetLength();
            var data = str->Bytes;
            for (var i = 0; i < len; i++)
                SendByte((char)data[i]);
        }

        public void Write(char c)
        {
            SendByte(c);
        }

        public void Write(uint value)
        {
            var str = value.ToString();
            Write(str);
            RuntimeMemory.FreeObject(str);
        }

        public void Write(int value)
        {
            var str = value.ToString();
            Write(str);
            RuntimeMemory.FreeObject(str);
        }

        public void WriteLine(string msg)
        {
            SendBytes(msg);
        }

        public void SetForegroundColor(byte color)
        {
            Write("\x001B[3");
            Write(color);
            Write("m");
        }

        public void SetBackgroundColor(byte color)
        {
            Write("\x001B[4");
            Write(color);
            Write("m");
        }

        public void SetCursor(int row, int column)
        {
            Write("\x001B[");
            Write(row);
            Write(";");
            Write(column);
            Write("H");
        }

        public void ApplyDefaultColor()
        {
            Write("\x001B[8]");
        }

    }

}
