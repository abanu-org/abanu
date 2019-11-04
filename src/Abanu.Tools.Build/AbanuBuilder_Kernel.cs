// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Net.Sockets;
using System.Threading;
using Abanu.Kernel.Core;
using Mosa.Compiler.Common;
using Mosa.Compiler.Framework.Linker;
using Mosa.Compiler.Framework.Linker.Elf;
using Mosa.Compiler.MosaTypeSystem;
using Mosa.Utility.BootImage;
using Mosa.Utility.Launcher;

namespace Abanu.Tools.Build
{
    public class AbanuBuilder_Kernel : AbanuBuilder
    {

        public AbanuBuilder_Kernel(string inputAssembly)
            : base(inputAssembly)
        {
        }

        public override void Configure()
        {
            InputAssembly = Env.Get("ABANU_EXE");

            Options = new LauncherOptions()
            {
                Emulator = EmulatorType.Bochs,
                ImageFormat = ImageFormat.IMG,
                //BootFormat = BootFormat.Multiboot_0_7,
                MultiBootV1 = true,
                PlatformType = Platform,
                LinkerFormatType = LinkerFormatType.Elf32,
                EmulatorMemoryInMB = 128,
                DestinationDirectory = Env.Get("ABANU_OSDIR"),
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
                BaseAddress = Address.KernelBaseVirt,
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
            //Options.EmitSymbols = true; // Kernel Loader needs to resolve address of Start Method
            //Options.Emitx86IRQMethods = true;
            Options.EmitAllSymbols = true;

            Options.EnableSSA = false;
            Options.EnableIROptimizations = false;
            Options.EnableSparseConditionalConstantPropagation = false;
            Options.EnableInlinedMethods = true;
            Options.InlineOnlyExplicit = true;
            Options.EnableLongExpansion = false;
            Options.EnableValueNumbering = false;
            Options.TwoPassOptimizations = false;
            Options.EnableBitTracker = false;
            Options.EnableLoopInvariantCodeMotion = false;
            Options.EnablePlatformOptimizations = false;
            Options.EnableMethodScanner = false;

            //Options.VBEVideo = true;

            Options.InterruptMethodName = "Abanu.Kernel.Core.IDT::ProcessInterrupt";

            Section sect = null;
            Options.CreateExtraSections = () =>
            {
                return new List<Section>
                {
                    new Section
                    {
                        Name = "native",
                        Type = SectionType.ProgBits,
                        AddressAlignment = 0x1000,
                        EmitMethod = (section, writer) =>
                        {
                            var data = File.ReadAllBytes(Env.Get("ABANU_NATIVE_FILES"));
                            writer.Write(data);
                            section.Size = (uint)data.Length;
                        },
                    },
                    new Section
                    {
                        Name = "consolefont.regular",
                        Type = SectionType.ProgBits,
                        AddressAlignment = 0x1000,
                        EmitMethod = (section, writer) =>
                        {
                            var data = File.ReadAllBytes(Path.Combine(Env.Get("ABANU_PROJDIR"), "tools", "consolefonts", "Uni2-Terminus14.psf"));
                            writer.Write(data);
                            section.Size = (uint)data.Length;
                        },
                    },
                    new Section
                    {
                        Name = "consolefont.bold",
                        Type = SectionType.ProgBits,
                        AddressAlignment = 0x1000,
                        EmitMethod = (section, writer) =>
                        {
                            var data = File.ReadAllBytes(Path.Combine(Env.Get("ABANU_PROJDIR"), "tools", "consolefonts", "Uni2-TerminusBold14.psf"));
                            writer.Write(data);
                            section.Size = (uint)data.Length;
                        },
                    },
                    new Section
                    {
                        Name = "App.HelloKernel",
                        Type = SectionType.ProgBits,
                        AddressAlignment = 0x1000,
                        EmitMethod = (section, writer) =>
                        {
                            var data = File.ReadAllBytes(Path.Combine(Env.Get("ABANU_PROJDIR"), "os", "App.HelloKernel.bin"));
                            writer.Write(data);
                            section.Size = (uint)data.Length;
                        },
                    },
                    //new Section
                    //{
                    //    Name = "Service.HostCommunication",
                    //    Type = SectionType.ProgBits,
                    //    AddressAlignment = 0x1000,
                    //    EmitMethod = (section, writer) =>
                    //    {
                    //        var data = File.ReadAllBytes(Path.Combine(Env.Get("ABANU_PROJDIR"), "os", "Abanu.Service.HostCommunication.bin"));
                    //        writer.Write(data);
                    //        section.Size = (uint)data.Length;
                    //    },
                    //},
                    new Section
                    {
                        Name = "App.Shell",
                        Type = SectionType.ProgBits,
                        AddressAlignment = 0x1000,
                        EmitMethod = (section, writer) =>
                        {
                            var data = File.ReadAllBytes(Path.Combine(Env.Get("ABANU_PROJDIR"), "os", "App.Shell.bin"));
                            writer.Write(data);
                            section.Size = (uint)data.Length;
                        },
                    },
                    new Section
                    {
                        Name = "App.HelloService",
                        Type = SectionType.ProgBits,
                        AddressAlignment = 0x1000,
                        EmitMethod = (section, writer) =>
                        {
                            var data = File.ReadAllBytes(Path.Combine(Env.Get("ABANU_PROJDIR"), "os", "App.HelloService.bin"));
                            writer.Write(data);
                            section.Size = (uint)data.Length;
                        },
                    },
                    new Section
                    {
                        Name = "Service.Basic",
                        Type = SectionType.ProgBits,
                        AddressAlignment = 0x1000,

                        EmitMethod = (section, writer) =>
                        {
                            var data = File.ReadAllBytes(Path.Combine(Env.Get("ABANU_PROJDIR"), "os", "Abanu.Service.Basic.bin"));
                            writer.Write(data);
                            section.Size = (uint)data.Length;
                        },
                    },
                    new Section
                    {
                        Name = "Service.ConsoleServer",
                        Type = SectionType.ProgBits,
                        AddressAlignment = 0x1000,

                        EmitMethod = (section, writer) =>
                        {
                            var data = File.ReadAllBytes(Path.Combine(Env.Get("ABANU_PROJDIR"), "os", "Abanu.Service.ConsoleServer.bin"));
                            writer.Write(data);
                            section.Size = (uint)data.Length;
                        },
                    },
                    new Section
                    {
                        Name = "elf.header",
                        Type = SectionType.ProgBits,
                        AddressAlignment = 0x1000,
                        Address = 0x4FF000,
                        Size = 0x1000,
                        EmitMethod = (section, writer) =>
                        {
                            sect = section; //TODO: Could set outside
                            writer.Write(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
                            section.Size = 0x1000;
                        },
                    },
                };
            };

            Options.CreateExtraProgramHeaders = () =>
            {
                return new List<ProgramHeader>
                 {
                    new ProgramHeader
                    {
                        Alignment = sect.AddressAlignment,
                        Offset = sect.Offset,
                        VirtualAddress = sect.Address,
                        PhysicalAddress = sect.Address,
                        FileSize = 0x1000,
                        MemorySize = 0x1000,
                        Type = ProgramHeaderType.Load,
                        Flags = ProgramHeaderFlags.Read,
                    },
                 };
            };
        }

    }
}
