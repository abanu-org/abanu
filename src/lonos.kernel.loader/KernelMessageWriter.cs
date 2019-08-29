using lonos.Kernel.Core;

namespace lonos.Kernel.Loader
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
