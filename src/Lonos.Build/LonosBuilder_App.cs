// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Net.Sockets;
using System.Threading;
using Lonos.Kernel.Core;
using Mosa.Compiler.Common;
using Mosa.Compiler.Framework.Linker;
using Mosa.Compiler.Framework.Linker.Elf;
using Mosa.Compiler.MosaTypeSystem;
using Mosa.Utility.BootImage;
using Mosa.Utility.Launcher;

namespace Lonos.Build
{
    public class LonosBuilder_App : LonosBuilder
    {

        public LonosBuilder_App(string inputAssembly)
            : base(inputAssembly)
        {
        }

        public override void Configure()
        {
            Options = new LauncherOptions()
            {
                Emulator = EmulatorType.Bochs,
                ImageFormat = ImageFormat.IMG,
                //BootFormat = BootFormat.Multiboot_0_7,
                MultiBootV1 = true,
                PlatformType = PlatformType.x86,
                LinkerFormatType = LinkerFormatType.Elf32,
                EmulatorMemoryInMB = 128,
                DestinationDirectory = Env.Get("LONOS_OSDIR"),
                FileSystem = FileSystem.FAT16,

                //UseMultiThreadingCompiler = false,
                EnableMultiThreading = false,

                InlinedIRMaximum = 12,
                BootLoader = BootLoader.Syslinux_3_72,
                VBEVideo = false,
                Width = 640,
                Height = 480,
                Depth = 32,
                //BaseAddress = 0x00500000,
                BaseAddress = Address.AppBaseVirt,
                //EmitRelocations = false,
                //EmitSymbols = false,
                //Emitx86IRQMethods = true,
                //SerialConnectionOption = SerialConnectionOption.Pipe,
                // EmitAllSymbols = true,
                // EnableQemuGDB = true,
                SerialConnectionPort = 9999,
                SerialConnectionHost = "127.0.0.1",
                SerialPipeName = "MOSA",
                ExitOnLaunch = true,
                GenerateNASMFile = false,
                GenerateASMFile = false,
                GenerateMapFile = false,
                GenerateDebugFile = false,
                PlugKorlib = true,
                HuntForCorLib = true,
            };

            //Options.GenerateNASMFile = true;
            Options.GenerateASMFile = true;
            Options.GenerateMapFile = true;
            Options.GenerateDebugFile = true;
            //Options.EmitRelocations = true;
            //Options.EmitSymbols = true; // Kernel Loader needs to resolve Adress of Start Method
            //Options.Emitx86IRQMethods = true;
            Options.EmitAllSymbols = true;

            Options.EnableSSA = true;
            Options.EnableIROptimizations = true;
            Options.EnableSparseConditionalConstantPropagation = false;
            Options.EnableInlinedMethods = false;
            Options.EnableLongExpansion = false;
            Options.EnableValueNumbering = false;
            Options.TwoPassOptimizations = false;
            //Options.EnableMethodScanner = true;

            //Options.VBEVideo = true;
        }

    }
}
