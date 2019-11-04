// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System;
using Abanu.Kernel.Core;

namespace Abanu.Kernel.Loader
{

    public static class KernelMessage_
    {

        public static void WriteLine(string msg)
        {
            Serial.Write(Serial.COM1, msg);
        }

        public static void WriteLine(StringBuffer msg)
        {
            for (var i = 0; i < msg.Length; i++)
                Serial.Write(Serial.COM1, (byte)msg[i]);
            Serial.Write(Serial.COM1, (byte)'\n');
        }

        public static void WriteLine(uint num)
        {
            var sb = new StringBuffer();
            sb.Append(num, 16);
            WriteLine(sb);
        }

    }

}
