// Copyright (c) MOSA Project. Licensed under the New BSD License.

using System;

namespace lonos.build
{
	internal static class Program
	{
		private static void Main()
		{
 			Console.WriteLine("Starting Build...");

			var file = "";

			//file = "Mosa.HelloWorld.x86.exe";
			file = "lonos.kernel.core.exe";
			//file = "Mosa.UnitTests.x86.exe";

			var engine = new LonosBuilder(file);
			engine.LaunchVirtualMachine();
			System.Console.WriteLine("ready");
			System.Console.ReadLine();
		}
	}
}
