using System;
namespace lonos.kernel.core
{

    public static class Devices
    {

        public static IFile COM1;
        public static IFile Screen;
        public static IFile Null;
        public static IFile KMsg;

        /// <summary>
        /// Pseudeo devices 
        /// </summary>
        public static void InitStage1()
        {
            Null = new NullDevice();
            KMsg = new KernelMessageDevice();
        }

        /// <summary>
        /// Output and Debug devices
        /// </summary>
        public static void InitStage2()
        {
            Serial.SetupPort(Serial.COM1);
            COM1 = new SerialDevice(Serial.COM1);

            lonos.kernel.core.Screen.EarlyInitialization();
            Screen = new ScreenDevice();
        }

        public static IFile GetDevice(string devName)
        {
            switch (devName)
            {
                case "/dev/ttyS0":
                    return COM1;
                case "/dev/tty":
                    return Screen;
                case "/dev/null":
                    return Null;
                case "/dev/kmsg":
                    return KMsg;
                default:
                    return null;
            }
        }

    }
}
