// Copyright (c) MOSA Project. Licensed under the New BSD License.

using Mosa.Compiler.Framework.Linker;
using Mosa.Compiler.MosaTypeSystem;
using Mosa.Utility.BootImage;
using Mosa.Utility.Launcher;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Net.Sockets;
using System.Threading;

namespace lonos.build
{
	public class LonosBuilder : IBuilderEvent, IStarterEvent
	{
		public Options Options { get; }
		public string TestAssemblyPath { get; set; }
		public string Platform { get; set; }
		public string InputAssembly { get; set; }
		public AppLocations AppLocations { get; set; }

		public TypeSystem TypeSystem { get; internal set; }
		public BaseLinker Linker { get; internal set; }

		protected Starter Starter;
		protected Process Process;
		protected string ImageFile;

		private Thread ProcessThread;

		public LonosBuilder(string inputAssembly)
		{
			Options = new Options() {
				EnableSSA = true,
				EnableIROptimizations = true,
				EnableSparseConditionalConstantPropagation = true,
				EnableInlinedMethods = true,
				EnableIRLongExpansion = true,
				EnableValueNumbering = true,
				TwoPassOptimizations = true,

				Emulator = EmulatorType.Bochs,
				ImageFormat = ImageFormat.IMG,
				BootFormat = BootFormat.Multiboot_0_7,
				PlatformType = PlatformType.X86,
				LinkerFormatType = LinkerFormatType.Elf32,
				EmulatorMemoryInMB = 128,
				DestinationDirectory = Path.Combine(Path.GetTempPath(), "MOSA-UnitTest"),
				FileSystem = FileSystem.FAT16,
				UseMultiThreadingCompiler = false,
				InlinedIRMaximum = 12,
				BootLoader = BootLoader.Syslinux_3_72,
				VBEVideo = false,
				Width = 640,
				Height = 480,
				Depth = 32,
				BaseAddress = 0x00500000,
				EmitRelocations = false,
				EmitSymbols = false,
				Emitx86IRQMethods = true,
				//SerialConnectionOption = SerialConnectionOption.Pipe,
				SerialConnectionPort = 9999,
				SerialConnectionHost = "127.0.0.1",
				SerialPipeName = "MOSA",
				ExitOnLaunch = true,
				GenerateNASMFile = false,
				GenerateASMFile = false,
				GenerateMapFile = false,
				GenerateDebugFile = false,
			};

			Options.GenerateNASMFile = true;
			Options.GenerateASMFile = true;
			Options.GenerateMapFile = true;
			Options.GenerateDebugFile = true;
			Options.EmitRelocations =  true;
			Options.EmitSymbols =  true;
			Options.Emitx86IRQMethods = true;

			Options.EnableSSA = false;
			Options.EnableIROptimizations = false;
			Options.EnableSparseConditionalConstantPropagation = false;
			Options.EnableInlinedMethods = false;
			Options.EnableIRLongExpansion = false;
			Options.EnableValueNumbering = false;
			Options.TwoPassOptimizations = false;

			AppLocations = new AppLocations();

			AppLocations.FindApplications();

			InputAssembly = inputAssembly;

			Initialize();
		}

		private void Initialize()
		{
			if (Platform == null) {
				Platform = "x86";
			}

			if (TestAssemblyPath == null) {
#if __MonoCS__
				TestAssemblyPath = AppDomain.CurrentDomain.BaseDirectory;
#else
				TestAssemblyPath = AppContext.BaseDirectory;
#endif
			}

			Compile();

		}

		public bool Compile()
		{
			Options.Paths.Add(TestAssemblyPath);
			Options.SourceFile = Path.Combine(TestAssemblyPath, InputAssembly);

			var builder = new Builder(Options, AppLocations, this);

			var start = DateTime.UtcNow;
			builder.Compile();
			Console.WriteLine((DateTime.UtcNow - start).ToString());

			Linker = builder.Linker;
			TypeSystem = builder.TypeSystem;
			ImageFile = Options.BootLoaderImage ?? builder.ImageFile;

			return !builder.HasCompileError;
		}

		public bool LaunchVirtualMachine()
		{
			if (Starter == null) {
				Options.ImageFile = ImageFile;
				Starter = new Starter(Options, AppLocations, this);
			}

			Options.SerialConnectionPort++;

			Process = Starter.Launch();

			return Process != null || !Process.HasExited;
		}

		public bool StartEngine()
		{
			Console.Write("Starting Engine...");

			if (StartEngineEx()) {
				Console.WriteLine();
				return true;
			} else {
			}

			Thread.Sleep(250);
			return true;
		}

		private bool StartEngineEx()
		{
			if (!LaunchVirtualMachine())
				return false;

			return true;
		}


		void IBuilderEvent.NewStatus(string status)
		{
			Console.WriteLine(status);
		}

		DateTime date = DateTime.UtcNow;
		void IBuilderEvent.UpdateProgress(int total, int at)
		{
			var d = DateTime.UtcNow;
			if (d.Second != date.Second) {
				Console.WriteLine(total + " / " + at);
			}
			date = d;
		}

		void IStarterEvent.NewStatus(string status)
		{
			//Console.WriteLine(status);
		}

	}
}
