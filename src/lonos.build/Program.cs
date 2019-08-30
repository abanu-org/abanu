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
                        value = "${LONOS_PROJDIR}/bin/x86/lonos.native.o";
                        break;
                    case "LONOS_BOOTLOADER_EXE":
                        value = "${LONOS_PROJDIR}/bin/lonos.os.loader.x86.exe";
                        break;
                    case "LONOS_EXE":
                        value = "${LONOS_PROJDIR}/bin/lonos.os.core.x86.exe";
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

            var regex = new Regex(@"\$\{(\w+)\}", RegexOptions.RightToLeft);

            if (string.IsNullOrEmpty(value))
                value = name;

            if (!string.IsNullOrEmpty(value))
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
            else if (args[0] == "--image=apps")
            {
                file = GetEnv("${LONOS_PROJDIR}/bin/app.hellokernel.exe");

                var builder = new LonosBuilder_App(file);
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
            var loaderFile = Path.Combine(Program.GetEnv("LONOS_OSDIR"), "lonos.os.loader.x86.bin");
            loaderFile = "/home/sebastian/projects/lonos/os/lonos.os.loader.x86.bin";

            var kernelFile = Path.Combine(Program.GetEnv("LONOS_OSDIR"), "lonos.os.core.x86.bin");
            kernelFile = "/home/sebastian/projects/lonos/os/lonos.os.core.x86.bin";

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
            var outFile = Path.Combine(Program.GetEnv("LONOS_OSDIR"), "lonos.os.image.x86.bin");
            File.WriteAllBytes(outFile, bytes);

        }

        public static uint DivCeil(uint value, uint dividor)
        {
            return (value - 1) / dividor + 1;
        }

    }
}
