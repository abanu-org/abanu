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

using Mosa.Compiler.Framework.Linker.Elf;
using Mosa.Compiler.Common;

namespace lonos.build
{
    public class LonosBuilder_Loader : IBuilderEvent, IStarterEvent
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

        public LonosBuilder_Loader(string inputAssembly)
        {
            Options = new Options()
            {
                EnableSSA = false,
                EnableIROptimizations = false,
                EnableSparseConditionalConstantPropagation = false,
                EnableInlinedMethods = false,
                EnableIRLongExpansion = false,
                EnableValueNumbering = false,
                TwoPassOptimizations = false,

                Emulator = EmulatorType.Bochs,
                ImageFormat = ImageFormat.IMG,
                BootFormat = BootFormat.Multiboot_0_7,
                PlatformType = PlatformType.X86,
                LinkerFormatType = LinkerFormatType.Elf32,
                EmulatorMemoryInMB = 128,
                DestinationDirectory = Program.GetEnv("LONOS_OSDIR"),
                FileSystem = FileSystem.FAT16,
                UseMultiThreadingCompiler = false,
                InlinedIRMaximum = 12,
                BootLoader = BootLoader.Syslinux_3_72,
                VBEVideo = false,
                BaseAddress = 0x00500000,
                EmitRelocations = false,
                EmitSymbols = false,
                Emitx86IRQMethods = false,
                //SerialConnectionOption = SerialConnectionOption.Pipe,
                ExitOnLaunch = true,
                GenerateNASMFile = false,
                GenerateASMFile = false,
                GenerateMapFile = true,
                GenerateDebugFile = false,
            };


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
                            var data = File.ReadAllBytes(Program.GetEnv("LONOS_NATIVE_FILES"));
                            writer.Write(data);
                            section.Size = (uint)data.Length;
                        }
                    },
                    /*new Section
                    {
                        Name = "boot.data",
                        Type = SectionType.ProgBits,
                        AddressAlignment = 0x1000,
                        Address = 0x4FF000,
                        Size=0x1000,
                        EmitMethod = (section, writer) =>
                        {
                            sect = section; //TODO: Could set outsite
                            writer.Write(new byte[]{1,2,3,4,5,6,7,8,9});
                            section.Size=0x1000;
                        }
                    }*/
                };
            };

            Options.CreateExtraProgramHeaders = () =>
             {
                 return new List<ProgramHeader>
                 {
                    /*new ProgramHeader
                    {
                        Alignment = sect.AddressAlignment,
                        Offset = sect.Offset,
                        VirtualAddress = sect.Address,
                        PhysicalAddress = sect.Address,
                        FileSize = 0x1000,
                        MemorySize = 0x1000,
                        Type = ProgramHeaderType.Load,
                        Flags = ProgramHeaderFlags.Read
                    }*/

                    // ELF Header. Reusing existing Region in File.
                    // This is allowed (overlapping Sections)
                    new ProgramHeader
                    {
                        Alignment = 0x1000,
                        Offset = 0x12345678,
                        FileSize = 0x12345678,
                        MemorySize = 0x100000,
                        PhysicalAddress = 0x05000000, //Multiboot will load section here
                        VirtualAddress = 0x05000000, 
                        Type = ProgramHeaderType.Load,
                        Flags = ProgramHeaderFlags.Read
                    }

                 };
             };

            AppLocations = new AppLocations();

            AppLocations.FindApplications();

            InputAssembly = inputAssembly;
        }

        public bool Build()
        {
            TestAssemblyPath = AppContext.BaseDirectory;
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

        void IBuilderEvent.NewStatus(string status)
        {
            Console.WriteLine(status);
        }

        DateTime date = DateTime.UtcNow;
        void IBuilderEvent.UpdateProgress(int total, int at)
        {
            var d = DateTime.UtcNow;
            if (d.Second != date.Second)
            {
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
