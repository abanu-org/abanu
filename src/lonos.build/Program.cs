// Copyright (c) MOSA Project. Licensed under the New BSD License.

using System;
using System.Text.RegularExpressions;
using System.IO;

namespace lonos.Build
{
    internal static class Program
    {

        public static string GetEnv(string name)
        {
            var value = Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrEmpty(value))
            {
                switch (name)
                {
                    case "LONOS_PROJDIR":
                        value = Path.GetDirectoryName(Path.GetDirectoryName(new Uri(typeof(Program).Assembly.Location).AbsolutePath));
                        break;
                    case "LONOS_OSDIR":
                        value = "${LONOS_PROJDIR}/os";
                        break;
                    case "LONOS_NATIVE_FILES":
                        value = "${LONOS_PROJDIR}/bin/lonos.native.o";
                        break;
                    case "LONOS_BOOTLOADER_EXE":
                        value = "${LONOS_PROJDIR}/bin/lonos.kernel.loader.exe";
                        break;
                    case "LONOS_EXE":
                        value = "${LONOS_PROJDIR}/bin/lonos.kernel.core.exe";
                        break;
                    case "LONOS_LOGDIR":
                        value = "${LONOS_PROJDIR}/logs";
                        break;
                    case "LONOS_ISODIR":
                        value = "${LONOS_PROJDIR}/iso";
                        break;
                    case "LONOS_TOOLSDIR":
                        value = "${LONOS_PROJDIR}/tools";
                        break;
                }
            }

            if (string.IsNullOrEmpty(value))
                return "";

            var regex = new Regex(@"\$\{(\w+)\}", RegexOptions.RightToLeft);
            foreach (Match m in regex.Matches(value))
                value = value.Replace(m.Value, GetEnv(m.Groups[1].Value));
            return value;
        }

        private static void Main(string[] args)
        {
            Console.WriteLine("Starting Build...");

            var file = "";

            //file = "Mosa.HelloWorld.x86.exe";
            var dir = Environment.CurrentDirectory;

            if (args[0] == "--image=loader")
            {
                file = GetEnv("LONOS_BOOTLOADER_EXE");

                var builderBoot = new LonosBuilder_Loader(file);
                builderBoot.Build();
            }
            else if (args[0] == "--image=kernel")
            {
                file = GetEnv("LONOS_EXE");

                var builder = new LonosBuilder_Kernel(file);
                builder.Build();
            }
            else if (args[0] == "--image=image")
            {
                LinkImages();
            }
            System.Console.WriteLine("ready");
            //System.Console.ReadLine();
        }


        public static void LinkImages()
        {
            var loaderFile = Path.Combine(Program.GetEnv("LONOS_OSDIR"), "lonos.kernel.loader.bin");
            loaderFile = "/home/sebastian/projects/lonos/os/lonos.kernel.loader.bin";

            var kernelFile = Path.Combine(Program.GetEnv("LONOS_OSDIR"), "lonos.kernel.core.bin");
            kernelFile = "/home/sebastian/projects/lonos/os/lonos.kernel.core.bin";

            var kernelBytes = File.ReadAllBytes(kernelFile);

            //var ms = new MemoryStream(File.ReadAllBytes(loaderFile));
            var ms = new MemoryStream();
            var reader = new BinaryReader(ms);
            var writer = new BinaryWriter(ms);
            writer.Write(File.ReadAllBytes(loaderFile));

            var alignment = 0x1000;

            var alignLoaderSectionStart = DivCeil((uint)ms.Length, (uint)alignment) * alignment;
            var fillBytes = alignLoaderSectionStart - ms.Length;
            for (var i = 0; i < fillBytes; i++)
                writer.Write((byte)0);

            var debugPos = ms.Length;
            writer.Write(kernelBytes);


            fillBytes = DivCeil((uint)kernelBytes.Length, (uint)alignment) * alignment - kernelBytes.Length;
            for (var i = 0; i < fillBytes; i++)
                writer.Write((byte)0);

            ms.Position = 42;
            var progHeaderSize = reader.ReadUInt16();
            ms.Position = 44;
            var progHeaderNum = reader.ReadUInt16();
            ms.Position = 28;
            var pHeaderArrayStart = reader.ReadUInt32();
            var extraEntry = pHeaderArrayStart + ((progHeaderNum - 1) * progHeaderSize);

            uint offset = (uint)alignLoaderSectionStart;
            uint memSize = (uint)kernelBytes.Length;
            uint fileSize = (uint)kernelBytes.Length;

            ms.Position = extraEntry + 4;
            writer.Write(offset);

            ms.Position = extraEntry + 16;
            writer.Write(fileSize);
            writer.Write(memSize);

            var bytes = ms.ToArray();
            var outFile = Path.Combine(Program.GetEnv("LONOS_OSDIR"), "lonos.kernel.image.bin");
            File.WriteAllBytes(outFile, bytes);

        }

        public static uint DivCeil(uint value, uint dividor)
        {
            return (value - 1) / dividor + 1;
        }


        /*
        private static void CreateDiskImage(string compiledFile)
        {
            var bootImageOptions = new BootImageOptions();

            if (Options.BootLoader == BootLoader.Syslinux_6_03)
            {
                bootImageOptions.MBRCode = GetResource(@"syslinux\6.03", "mbr.bin");
                bootImageOptions.FatBootCode = GetResource(@"syslinux\6.03", "ldlinux.bin");

                bootImageOptions.IncludeFiles.Add(new IncludeFile("ldlinux.sys", GetResource(@"syslinux\6.03", "ldlinux.sys")));
                bootImageOptions.IncludeFiles.Add(new IncludeFile("mboot.c32", GetResource(@"syslinux\6.03", "mboot.c32")));
            }
            else if (Options.BootLoader == BootLoader.Syslinux_3_72)
            {
                bootImageOptions.MBRCode = GetResource(@"syslinux\3.72", "mbr.bin");
                bootImageOptions.FatBootCode = GetResource(@"syslinux\3.72", "ldlinux.bin");

                bootImageOptions.IncludeFiles.Add(new IncludeFile("ldlinux.sys", GetResource(@"syslinux\3.72", "ldlinux.sys")));
                bootImageOptions.IncludeFiles.Add(new IncludeFile("mboot.c32", GetResource(@"syslinux\3.72", "mboot.c32")));
            }

            bootImageOptions.IncludeFiles.Add(new IncludeFile("syslinux.cfg", GetResource("syslinux", "syslinux.cfg")));
            bootImageOptions.IncludeFiles.Add(new IncludeFile(compiledFile, "main.exe"));

            bootImageOptions.IncludeFiles.Add(new IncludeFile("TEST.TXT", Encoding.ASCII.GetBytes("This is a test file.")));

            foreach (var include in Options.IncludeFiles)
            {
                bootImageOptions.IncludeFiles.Add(include);
            }

            bootImageOptions.VolumeLabel = "MOSABOOT";

            var vmext = ".img";
            switch (Options.ImageFormat)
            {
                case ImageFormat.VHD: vmext = ".vhd"; break;
                case ImageFormat.VDI: vmext = ".vdi"; break;
                default: break;
            }

            ImageFile = Path.Combine(Options.DestinationDirectory, Path.GetFileNameWithoutExtension(Options.SourceFile) + vmext);

            bootImageOptions.DiskImageFileName = ImageFile;
            bootImageOptions.PatchSyslinuxOption = true;
            bootImageOptions.FileSystem = Options.FileSystem;
            bootImageOptions.ImageFormat = Options.ImageFormat;
            bootImageOptions.BootLoader = Options.BootLoader;

            Generator.Create(bootImageOptions);
        }*/

    }
}
