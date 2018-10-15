using System;
using Mosa.Runtime;
using Mosa.Kernel.x86;

namespace lonos.kernel.core
{

	public static class Boot
	{

		public static void Main()
		{
			RawWrite(0, 7, 'U', 1);
			IDT.SetInterruptHandler(null);
			//Panic.Setup();
			//PIC.Setup();
			IDT.Setup();
			//Multiboot.Setup();

			//Panic.DumpMemory(Address.GDTTable);

			//Memory.Init();
			         

			RawWrite(0, 0, 'A', 1);
			USize size = 5;
			RawWrite(0, size, 'B', 1);
			RawWrite(0, 2, 'C', 1);
			while (true) {
				Mosa.Runtime.Intrinsic.Load8(IntPtr.Zero);
			};
		}

		private static void Dummy(){
			//This is a dummy call, that get never executed.
			//Its requied, because we need a real reference to Mosa.Runtime.x86
			//Without that, the .NET compiler will optimize that reference away
			//if its nowhere used. Than the Compiler dosnt know about that Refernce
			//and the Compilation will fail
			Mosa.Runtime.x86.Internal.GetStackFrame(0);
		}

		public const uint Columns = 80;

		/// <summary>
		/// The rows
		/// </summary>
		public const uint Rows = 40;

		public static void RawWrite(uint row, uint column, char chr, byte color)
		{
			IntPtr address = new IntPtr(0x0B8000 + ((row * Columns + column) * 2));

			Intrinsic.Store8(address, (byte)chr);
			Intrinsic.Store8(address, 1, color);
		}

	}
}
