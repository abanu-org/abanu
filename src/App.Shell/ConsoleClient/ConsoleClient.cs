// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Abanu;
using Abanu.Kernel.Core;
using Abanu.Runtime;

namespace Abanu.Kernel
{

    public class ConsoleClient
    {

        private Stream Stream;

        public ConsoleClient(Stream stream)
        {
            Stream = stream;
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

        private unsafe void SendByte(char data)
        {
            Stream.WriteByte((byte)data);
        }

        private void SendBytes(string data)
        {
            for (var i = 0; i < data.Length; i++)
            {
                SendByte(data[i]);
            }
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
            Write("\n");
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
