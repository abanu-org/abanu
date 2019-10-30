// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Mosa.Compiler.MosaTypeSystem;

namespace Abanu.Tools.Build
{

    internal static class Program
    {

        private static void Main(string[] args)
        {
            Directory.CreateDirectory(Env.Get("${ABANU_LOGDIR}"));

            if (args.Length == 0 && Debugger.IsAttached)
            {
                //Verb("build assembly");
                Verb("build --native --bin=all");
                Verb("build --image");
                Verb("run --emulator=qemu --boot=direct");
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
            Console.WriteLine("Command: " + args.ToString());
            switch (args[0])
            {
                case "build":
                    return Build(args.Pop());
                case "run":
                    return Run(args.Pop());
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
                Directory.CreateDirectory(Env.Get("${ABANU_BINDIR}/${ABANU_ARCH}"));
                Exec("${nasm} -f elf32 ${ABANU_PROJDIR}/src/Abanu.Native.${ABANU_ARCH}/DebugFunction1.s -o ${ABANU_BINDIR}/${ABANU_ARCH}/Abanu.Native.o");
                Exec("${nasm} -f bin ${ABANU_PROJDIR}/src/Abanu.Native.${ABANU_ARCH}/EnableExecutionProtection.s -o ${ABANU_BINDIR}/${ABANU_ARCH}/Abanu.EnableExecutionProtection.o");
                Exec("${nasm} -f bin ${ABANU_PROJDIR}/src/Abanu.Native.${ABANU_ARCH}/InterruptReturn.s -o ${ABANU_BINDIR}/${ABANU_ARCH}/Abanu.InterruptReturn.o");
                Exec("${nasm} -f bin ${ABANU_PROJDIR}/src/Abanu.Native.${ABANU_ARCH}/LoadTaskRegister.s -o ${ABANU_BINDIR}/${ABANU_ARCH}/Abanu.LoadTaskRegister.o");
                Exec("${nasm} -f bin ${ABANU_PROJDIR}/src/Abanu.Native.${ABANU_ARCH}/DebugFunction1.s -o ${ABANU_BINDIR}/${ABANU_ARCH}/Abanu.DebugFunction1.o");
                Exec("${nasm} -f bin ${ABANU_PROJDIR}/src/Abanu.Native.${ABANU_ARCH}/SysCall.s -o ${ABANU_BINDIR}/${ABANU_ARCH}/Abanu.SysCall.o");
            }
            return null;
        }

        private static CommandResult TryBuildBin(CommandArgs args)
        {
            var possibleImages = new string[] { "all", "app", "app2", "service.hostcommunication", "service.basic", "service.consoleserver", "app.shell", "loader", "kernel", "external" };
            var image = args.GetFlag("bin", possibleImages);
            var images = image.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            if (image == "all")
                images = possibleImages;

            var newArgs = args.RemoveFlag("bin");

            foreach (var img in images.Where(s => s != "all"))
                BuildImage(img + " " + newArgs);
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

        private static CommandResult Run(CommandArgs args)
        {
            switch (args.GetFlag("emulator", "qemu"))
            {
                case "qemu":
                    return RunQemu(args);

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
            Env.Set("DEBUG_INTERRUPTS", ",int");
            switch (args.RequireFlag("boot", "direct"))
            {
                case "direct":
                    using (var qemu = ExecAsync("${qemu} -kernel ${ABANU_OSDIR}/Abanu.OS.image.${ABANU_ARCH}.bin -serial file:${ABANU_LOGDIR}/kernel.log -d pcall,cpu_reset,guest_errors${DEBUG_INTERRUPTS} -D ${ABANU_LOGDIR}/emulator.log -s -S -m 256"))
                    {
                        Thread.Sleep(500);
                        using (var gdb = ExecAsync("${gdb}", false))
                        {
                            gdb.WaitForExit();
                            break;
                        }
                    }
            }
            return null;
        }

        private static CommandResult RunQemu(CommandArgs args)
        {
            Env.Set("DEBUG_INTERRUPTS", ",int");
            switch (args.RequireFlag("boot", "direct"))
            {
                case "direct":
                    Exec("${qemu} -kernel ${ABANU_OSDIR}/Abanu.OS.image.${ABANU_ARCH}.bin -serial file:${ABANU_LOGDIR}/kernel.log -d pcall,cpu_reset,guest_errors${DEBUG_INTERRUPTS} -D ${ABANU_LOGDIR}/emulator.log -m 256");
                    break;
            }
            return null;
        }

        private static ProcessResult Exec(CommandArgs args)
        {
            using (var result = ExecAsync(args, true))
            {
                result.WaitForExit();
                result?.Dispose();
                return result;
            }
        }

        private static ProcessResult ExecAsync(CommandArgs args)
        {
            return ExecAsync(args, false);
        }

        private static ProcessResult ExecAsync(CommandArgs args, bool redirect)
        {
            if (!args.IsSet())
                return null;

            var fileName = args[0].GetEnv();
            var arguments = args.Pop(1).ToString().GetEnv();

            var start = new ProcessStartInfo(fileName);

            if (arguments.Length > 0)
                start.Arguments = arguments;

            if (redirect)
            {
                start.RedirectStandardOutput = true;
                start.RedirectStandardError = true;
                start.UseShellExecute = false;
            }

            Console.WriteLine(fileName + " " + arguments);

            var proc = Process.Start(start);
            if (redirect)
            {
                var data = proc.StandardOutput.ReadToEnd();
                var error = proc.StandardError.ReadToEnd();
                Console.WriteLine(data);
                Console.WriteLine(error);
            }

            return new ProcessResult(proc);
        }

        private static CommandResult BuildImage(CommandArgs args)
        {
            Console.WriteLine("Starting Build...");

            string file;
            if (args[0] == "loader")
            {
                file = Env.Get("ABANU_BOOTLOADER_EXE");

                var builderBoot = new AbanuBuilder_Loader(file);
                builderBoot.Build();
            }
            else if (args[0] == "kernel")
            {
                file = Env.Get("ABANU_EXE");

                var builder = new AbanuBuilder_Kernel(file);
                builder.Build();
            }
            else if (args[0] == "app")
            {
                file = Env.Get("${ABANU_PROJDIR}/bin/App.HelloKernel.exe");

                var builder = new AbanuBuilder_App(file);
                builder.Build();
            }
            else if (args[0] == "app2")
            {
                file = Env.Get("${ABANU_PROJDIR}/bin/App.HelloService.exe");

                var builder = new AbanuBuilder_App(file);
                builder.Build();
            }
            else if (args[0] == "service.consoleserver")
            {
                file = Env.Get("${ABANU_PROJDIR}/bin/Abanu.Service.ConsoleServer.exe");

                var builder = new AbanuBuilder_App(file);
                builder.Build();
            }
            else if (args[0] == "service.basic")
            {
                file = Env.Get("${ABANU_PROJDIR}/bin/Abanu.Service.Basic.exe");

                var builder = new AbanuBuilder_App(file);
                builder.Build();
            }
            else if (args[0] == "external")
            {
                file = args[1];

                var builder = new AbanuBuilder_App(file);
                builder.Build();
            }
            else if (args[0] == "service.hostcommunication")
            {
                file = Env.Get("${ABANU_PROJDIR}/bin/Abanu.Service.HostCommunication.exe");

                var builder = new AbanuBuilder_App(file);
                builder.Build();
            }
            else if (args[0] == "app.shell")
            {
                file = Env.Get("${ABANU_PROJDIR}/bin/App.Shell.exe");

                var builder = new AbanuBuilder_App(file);
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
            var loaderFile = Env.Get("${ABANU_OSDIR}/Abanu.OS.Loader.${ABANU_ARCH}.bin");
            var kernelFile = Env.Get("${ABANU_OSDIR}/Abanu.OS.Core.${ABANU_ARCH}.bin");

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
                    var outFile = Env.Get("${ABANU_OSDIR}/Abanu.OS.image.${ABANU_ARCH}.bin");
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
