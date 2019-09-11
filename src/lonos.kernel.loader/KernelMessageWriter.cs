// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using Lonos.Kernel.Core;

namespace Lonos.Kernel.Loader
{
    public class KernelMessageWriter : IBufferWriter
    {

        public KernelMessageWriter()
        {
            Serial.SetupPort(Serial.COM1);
            Screen.EarlyInitialization();
            Screen.BackgroundColor = ScreenColor.DarkGray;
            Screen.Clear();
        }

        public unsafe SSize Write(byte* buf, USize count)
        {
            for (var i = 0; i < count; i++)
            {
                Serial.Write(Serial.COM1, buf[i]);
                Screen.Write((char)buf[i]);
            }
            return (uint)count;
        }

    }

}
