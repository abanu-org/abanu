// Copyright (c) MOSA Project. Licensed under the New BSD License.

using System;

namespace lonos.build
{
	internal static class Program
	{
		private static void Main()
		{
			Console.WriteLine("Starting Build...");
			var engine = new LonosBuilder();
			engine.LaunchVirtualMachine();
			System.Console.WriteLine("ready");
			System.Console.ReadLine();
		}
	}
}
