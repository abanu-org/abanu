using System;
using Mosa.Runtime;

namespace lonos.kernel.core
{

	public static class Boot
	{
		public static void Main()
		{
			RawWrite(0, 0, 'X', 0);
			while (true){} ;
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
