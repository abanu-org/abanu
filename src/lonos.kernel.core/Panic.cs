// Copyright (c) MOSA Project. Licensed under the New BSD License.

using Mosa.Runtime.x86;
using System;
using Mosa.Kernel.x86;

namespace lonos.kernel.core
{
    /// <summary>
    /// Panic
    /// </summary>
    public static class Panic
    {
        private static bool firstError = true;

        public static uint EBP = 0;
        public static uint EIP = 0;
        public static uint EAX = 0;
        public static uint EBX = 0;
        public static uint ECX = 0;
        public static uint EDX = 0;
        public static uint EDI = 0;
        public static uint ESI = 0;
        public static uint ESP = 0;
        public static uint Interrupt = 0;
        public static uint ErrorCode = 0;
        public static uint CS = 0;
        public static uint EFLAGS = 0;
        public static uint CR2 = 0;
        public static uint FS = 0;

        public static void Setup()
        {
        }

		private static void PrepareScreen(string title)
        {
            IDT.SetInterruptHandler(null);
			Screen.BackgroundColor = ScreenColor.Black;
            Screen.Clear();
            Screen.Goto(1, 1);
			Screen.Color = ScreenColor.LightGray;
            Screen.Write("*** ");
            Screen.Write(title);

            if (firstError)
                firstError = false;
            else
                Screen.Write(" (multiple)");

            Screen.Write(" ***");
            Screen.Goto(3, 1);
        }

        public static void Error(string message)
        {
            IDT.SetInterruptHandler(null);

            Screen.BackgroundColor = ScreenColor.Blue;

            Screen.Clear();
            Screen.Goto(1, 0);
            Screen.Color = ScreenColor.White;
            Screen.Write("*** Kernel Panic ***");

            if (firstError)
                firstError = false;
            else
                Screen.Write(" (multiple)");

            Screen.NextLine();
            Screen.NextLine();
            Screen.Write(message);
            Screen.NextLine();
            Screen.NextLine();
            Screen.Write("REGISTERS:");
            Screen.NextLine();
            Screen.NextLine();
            DumpRegisters();
            Screen.NextLine();
            Screen.Write("STACK TRACE:");
            Screen.NextLine();
            Screen.NextLine();
            DumpStackTrace();

            while (true)
            {
                // keep debugger running
                unsafe
                {
                    //Debugger.Process(null);
                }

                Native.Hlt();
            }
        }

        public static void DumpRegisters()
        {
            Screen.Write("EIP: ");
            Screen.Write(EIP, 16, 8);
            Screen.Write(" ESP: ");
            Screen.Write(ESP, 16, 8);
            Screen.Write(" EBP: ");
            Screen.Write(EBP, 16, 8);
            Screen.Write(" EFLAGS: ");
            Screen.Write(EFLAGS, 16, 8);
            Screen.Write(" CR2: ");
            Screen.Write(CR2, 16, 8);
            Screen.NextLine();
            Screen.Write("EAX: ");
            Screen.Write(EAX, 16, 8);
            Screen.Write(" EBX: ");
            Screen.Write(EBX, 16, 8);
            Screen.Write(" ECX: ");
            Screen.Write(ECX, 16, 8);
            Screen.Write(" CS: ");
            Screen.Write(CS, 16, 8);
            Screen.Write(" FS: ");
            Screen.Write(FS, 16, 8);
            Screen.NextLine();
            Screen.Write("EDX: ");
            Screen.Write(EDX, 16, 8);
            Screen.Write(" EDI: ");
            Screen.Write(EDI, 16, 8);
            Screen.Write(" ESI: ");
            Screen.Write(ESI, 16, 8);
            Screen.Write(" ERROR: ");
            Screen.Write(ErrorCode, 16, 2);
            Screen.Write(" IRQ: ");
            Screen.Write(Interrupt, 16, 2);
            Screen.NextLine();
        }

        public static void DumpStackTrace()
        {
            DumpStackTrace(0);
        }

        private static void DumpStackTrace(uint depth)
        {
            while (true)
            {
                var entry = Internal.GetStackTraceEntry(depth, new IntPtr(EBP), new IntPtr(EIP));

                if (!entry.Valid)
                    return;

                if (!entry.Skip)
                {
                    Screen.Write(entry.ToString());
                    Screen.Row++;
                    Screen.Column = 0;
                }

                depth++;
            }
        }

		#region DumpMemory

        public static void DumpMemory(uint address)
        {
            PrepareScreen("Memory Dump");
            Screen.Column = 0;
            Screen.Write("ADDRESS  ");
			Screen.Color = ScreenColor.Brown;
            Screen.Write("03 02 01 00  07 06 05 04   11 10 09 08  15 14 13 12   ASCII");

            var word = address;
            var rowAddress = address;
			uint rows = 10; //21;
            for (uint y = 0; y < rows; y++)
            {
                Screen.Row++;
                Screen.Column = 0;

				//WriteHex(word, 8, ScreenColor.Brown);
				Screen.Write(word, 8, -1);
                Screen.Write("  ");

                const uint dwordsPerRow = 4;
                for (uint x = 0; x < dwordsPerRow; x++)
                {
                    for (uint x2 = 0; x2 < 4; x2++)
                    {
                        var number = Native.Get8(word + ((4 - 1) - x2));
						//WriteHex(number, 2, ScreenColor.LightGray);
						Screen.Write(' ');
						Screen.Write(number, 16, 2);
						Screen.Write(' ');
                    }
                    if (x == 1 || x == 3)
                        Screen.Write(' ');
                    Screen.Write(' ');
                    word += 4;
                }

                

				//Ascii view
                for (uint x = 0; x < dwordsPerRow * 4; x++)
                {
                    var num = Native.Get8(rowAddress + x);
                    if (num == 0)
						Screen.Color = ScreenColor.DarkGray;
                    else
						Screen.Color = ScreenColor.LightGray;

                    if (num >= 32 && num < 128)
                    {
						Screen.Color = ScreenColor.LightGray;
                        Screen.Write((char)num);
                    }
                    else
                    {
                        if (num == 0)
							Screen.Color = ScreenColor.DarkGray;
                        else
							Screen.Color = ScreenColor.LightGray;
                        Screen.Write('.');
                    }
                }
				Screen.Color = ScreenColor.LightGray;

                //avoid empty line, when line before was fully filled
                if (Screen.Column == 0)
                    Screen.Row--;

                rowAddress += (dwordsPerRow * 4);
            }
			while(true){};
            //Halt();
        }

        private static void WriteHexChar(byte num)
        {
            if (num >= 32 && num < 128)
            {
				Screen.Color = ScreenColor.LightGray;
                Screen.Write((char)num);
            }
            else
            {
                if (num == 0)
					Screen.Color = ScreenColor.DarkGray;
                else
					Screen.Color = ScreenColor.LightGray;
                Screen.Write('.');
            }
        }

        private static void WriteHex(uint num, byte color)
        {
            WriteHex(num, 0, color);
        }

        private static void WriteHex(uint num, byte digits, byte color)
        {
            var oldColor = Screen.Color;
            Screen.Color = color;

            if (num == 0)
				Screen.Color = ScreenColor.LightGray;

            var hex = new StringBuffer(num, "X");

            for (var i = 0; i < digits - hex.Length; i++)
                Screen.Write('0');
			Screen.Write(hex);
			//Screen.Write(num, 3, -1);

            Screen.Color = oldColor;
        }

        #endregion DumpMemory

		#region Message

        public static void BeginMessage()
        {
            PrepareScreen("Debug Message");
			Screen.Color = ScreenColor.Red;
        }

        public static void Message(string message)
        {
            BeginMessage();
            Screen.Write(message);
            Halt();
        }

        public static void Message(char message)
        {
            BeginMessage();
            Screen.Write(message);
            Halt();
        }

        public static void Message(uint message)
        {
            BeginMessage();
            Screen.Write(" Number: 0x");
            Screen.Write(message, "X");
            Halt();
            
        }

        #endregion Message
    
		private static void Halt()
        {
            Screen.Goto(Screen.Rows - 1, 0);
            while (true)
                Native.Hlt();
        }
	}


}
