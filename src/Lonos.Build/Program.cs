// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Mosa.Compiler.MosaTypeSystem;

namespace Lonos.Build
{

    internal static class Program
    {

        private static void Main(string[] args)
        {
            Verb(CommandArgs.FromCommandlineArguments(args));
        }

        private static CommandResult Verb(CommandArgs args)
        {
            switch (args[0])
            {
                case "build":
                    return Build(args.Pop());
                case "debug":
                    return Debug(args.Pop());
            }
            return null;
        }

        private static CommandResult Build(CommandArgs args)
        {
            switch (args[0])
            {
                case "image":
                    return BuildImage(args.Pop());
            }
            return null;
        }

        private static CommandResult Debug(CommandArgs args)
        {
            if (args.ContainsFlag("emulator", "qemu"))
                return DebugQemu(args);
            return null;
        }

        private static CommandResult DebugQemu(CommandArgs args)
        {
            var direct = args.ContainsFlag("boot", "direct");
            if (direct)
                Exec("${qemu} -kernel os/Lonos.OS.image.x86.bin");
            return null;
        }

        private static Process Exec(CommandArgs args)
        {
            if (!args.IsSet())
                return null;

            var fileName = args[0].GetEnv();
            var arguments = args.Pop(1).ToString().GetEnv();

            var start = new ProcessStartInfo(fileName);

            if (arguments.Length > 0)
                start.Arguments = arguments;

            start.RedirectStandardOutput = true;
            start.RedirectStandardError = true;
            start.UseShellExecute = false;

            var proc = Process.Start(start);
            var data = proc.StandardOutput.ReadToEnd();
            var error = proc.StandardError.ReadToEnd();
            Console.WriteLine(data);
            Console.WriteLine(error);
            return proc;
        }

        private static CommandResult BuildImage(CommandArgs args)
        {
            Console.WriteLine("Starting Build...");

            string file;
            if (args[0] == "loader")
            {
                file = BuildUtility.GetEnv("LONOS_BOOTLOADER_EXE");

                var builderBoot = new LonosBuilder_Loader(file);
                builderBoot.Build();
            }
            else if (args[0] == "kernel")
            {
                file = BuildUtility.GetEnv("LONOS_EXE");

                var builder = new LonosBuilder_Kernel(file);
                builder.Build();
            }
            else if (args[0] == "app")
            {
                file = BuildUtility.GetEnv("${LONOS_PROJDIR}/bin/App.HelloKernel.exe");

                var builder = new LonosBuilder_App(file);
                builder.Build();
            }
            else if (args[0] == "app2")
            {
                file = BuildUtility.GetEnv("${LONOS_PROJDIR}/bin/App.HelloService.exe");

                var builder = new LonosBuilder_App(file);
                builder.Build();
            }
            else if (args[0] == "service.basic")
            {
                file = BuildUtility.GetEnv("${LONOS_PROJDIR}/bin/Lonos.Service.Basic.exe");

                var builder = new LonosBuilder_App(file);
                builder.Build();
            }
            else if (args[0] == "app.shell")
            {
                file = BuildUtility.GetEnv("${LONOS_PROJDIR}/bin/App.Shell.exe");

                var builder = new LonosBuilder_App(file);
                builder.Build();
            }
            else if (args[0] == "image")
            {
                LinkImages();
            }
            Console.WriteLine("ready");
            //System.Console.ReadLine();

            return null;
        }

        public static void LinkImages()
        {
            var loaderFile = BuildUtility.GetEnv("${LONOS_OSDIR}/Lonos.OS.Loader.x86.bin");
            var kernelFile = BuildUtility.GetEnv("${LONOS_OSDIR}/Lonos.OS.Core.x86.bin");

            var kernelBytes = File.ReadAllBytes(kernelFile);

            //var ms = new MemoryStream(File.ReadAllBytes(loaderFile));
            var ms = new MemoryStream();
            using (var reader = new BinaryReader(ms))
            {
                using (var writer = new BinaryWriter(ms))
                {
                    writer.Write(File.ReadAllBytes(loaderFile));

                    var alignment = 0x1000;

                    var alignLoaderSectionStart = DivCeil((uint)ms.Length, (uint)alignment) * alignment;
                    var fillBytes = alignLoaderSectionStart - ms.Length;
                    for (var i = 0; i < fillBytes; i++)
                        writer.Write((byte)0);

                    var debugPos = ms.Length;
                    writer.Write(kernelBytes);

                    fillBytes = (DivCeil((uint)kernelBytes.Length, (uint)alignment) * alignment) - kernelBytes.Length;
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
                    var outFile = Path.Combine(BuildUtility.GetEnv("LONOS_OSDIR"), "Lonos.OS.image.x86.bin");
                    File.WriteAllBytes(outFile, bytes);
                }
            }
        }

        public static uint DivCeil(uint value, uint dividor)
        {
            return ((value - 1) / dividor) + 1;
        }

    }
}
