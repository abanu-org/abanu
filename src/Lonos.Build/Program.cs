// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Mosa.Compiler.MosaTypeSystem;

namespace Lonos.Build
{

    internal static class Program
    {

        private static void Main(string[] args)
        {
            Directory.CreateDirectory(Env.Get("${LONOS_LOGDIR}"));

            if (args.Length == 0 && Debugger.IsAttached)
            {
                //Verb("build assembly");
                Verb("build --native --bin=all");
                Verb("build --image");
                Verb("debug --emulator=qemu --boot=direct");
            }
            else
            {
                Verb(CommandArgs.FromCommandlineArguments(args));
            }

            if (Debugger.IsAttached)
            {
                Console.WriteLine("Press any key to continue");
                Console.ReadKey();
            }
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
            TryBuildNative(args);
            TryBuildBin(args);
            TryBuildImage(args);
            return null;
        }

        private static CommandResult TryBuildNative(CommandArgs args)
        {
            if (args.ContainsFlag("native"))
            {
                Directory.CreateDirectory(Env.Get("${LONOS_BINDIR}/x86"));
                Exec("${nasm} -f elf32 ${LONOS_PROJDIR}/src/Lonos.Native.x86/DebugFunction1.s -o ${LONOS_BINDIR}/x86/Lonos.Native.o");
                Exec("${nasm} -f bin ${LONOS_PROJDIR}/src/Lonos.Native.x86/EnableExecutionProtection.s -o ${LONOS_BINDIR}/x86/Lonos.EnableExecutionProtection.o");
                Exec("${nasm} -f bin ${LONOS_PROJDIR}/src/Lonos.Native.x86/InterruptReturn.s -o ${LONOS_BINDIR}/x86/Lonos.InterruptReturn.o");
                Exec("${nasm} -f bin ${LONOS_PROJDIR}/src/Lonos.Native.x86/LoadTaskRegister.s -o ${LONOS_BINDIR}/x86/Lonos.LoadTaskRegister.o");
                Exec("${nasm} -f bin ${LONOS_PROJDIR}/src/Lonos.Native.x86/DebugFunction1.s -o ${LONOS_BINDIR}/x86/Lonos.DebugFunction1.o");
                Exec("${nasm} -f bin ${LONOS_PROJDIR}/src/Lonos.Native.x86/App.HelloKernel.s -o ${LONOS_BINDIR}/x86/App.HelloKernel.o");
            }
            return null;
        }

        private static CommandResult TryBuildBin(CommandArgs args)
        {
            var possibleImages = new string[] { "all", "app", "app2", "service.basic", "app.shell", "loader", "kernel" };
            var image = args.GetFlag("bin", possibleImages);
            var images = image.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            if (image == "all")
                images = possibleImages;
            foreach (var img in images.Where(s => s != "all"))
                BuildImage(img);
            return null;
        }

        private static CommandResult TryBuildImage(CommandArgs args)
        {
            if (args.ContainsFlag("image"))
                BuildImage("image");
            return null;
        }

        private static CommandResult Debug(CommandArgs args)
        {
            switch (args.GetFlag("emulator", "qemu"))
            {
                case "qemu":
                    return DebugQemu(args);

            }
            return null;
        }

        public static CommandResult Error(string message)
        {
            Console.WriteLine(message);
            return null;
        }

        private static CommandResult DebugQemu(CommandArgs args)
        {
            Env.Set("DEBUG_INTERRUPTS", "");
            switch (args.RequireFlag("boot", "direct"))
            {
                case "direct":
                    return Exec("${qemu} -kernel ${LONOS_OSDIR}/Lonos.OS.image.x86.bin -serial file:${LONOS_LOGDIR}/kernel.log -d pcall,cpu_reset,guest_errors${DEBUG_INTERRUPTS} -D ${LONOS_LOGDIR}/emulator.log");

            }
            return null;
        }

        private static CommandResult Exec(CommandArgs args)
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

            Console.WriteLine(fileName + " " + arguments);

            using (var proc = Process.Start(start))
            {
                var data = proc.StandardOutput.ReadToEnd();
                var error = proc.StandardError.ReadToEnd();
                Console.WriteLine(data);
                Console.WriteLine(error);
                proc.WaitForExit();
            }

            return null;
        }

        private static CommandResult BuildImage(CommandArgs args)
        {
            Console.WriteLine("Starting Build...");

            string file;
            if (args[0] == "loader")
            {
                file = Env.Get("LONOS_BOOTLOADER_EXE");

                var builderBoot = new LonosBuilder_Loader(file);
                builderBoot.Build();
            }
            else if (args[0] == "kernel")
            {
                file = Env.Get("LONOS_EXE");

                var builder = new LonosBuilder_Kernel(file);
                builder.Build();
            }
            else if (args[0] == "app")
            {
                file = Env.Get("${LONOS_PROJDIR}/bin/App.HelloKernel.exe");

                var builder = new LonosBuilder_App(file);
                builder.Build();
            }
            else if (args[0] == "app2")
            {
                file = Env.Get("${LONOS_PROJDIR}/bin/App.HelloService.exe");

                var builder = new LonosBuilder_App(file);
                builder.Build();
            }
            else if (args[0] == "service.basic")
            {
                file = Env.Get("${LONOS_PROJDIR}/bin/Lonos.Service.Basic.exe");

                var builder = new LonosBuilder_App(file);
                builder.Build();
            }
            else if (args[0] == "app.shell")
            {
                file = Env.Get("${LONOS_PROJDIR}/bin/App.Shell.exe");

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
            var loaderFile = Env.Get("${LONOS_OSDIR}/Lonos.OS.Loader.x86.bin");
            var kernelFile = Env.Get("${LONOS_OSDIR}/Lonos.OS.Core.x86.bin");

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
                    var outFile = Path.Combine(Env.Get("LONOS_OSDIR"), "Lonos.OS.image.x86.bin");
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
