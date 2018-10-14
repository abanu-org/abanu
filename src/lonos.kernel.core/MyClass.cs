using System;
using Mosa.Runtime;

namespace lonos.kernel.core
{

	public static class Boot
	{
		public static void Main()
		{
			System.Collections.Generic.List<int> m = new System.Collections.Generic.List<int>();
			RawWrite(0, 0, 'A', 1);
			while (true) { };
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
