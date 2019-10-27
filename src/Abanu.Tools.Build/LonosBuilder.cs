// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

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

#pragma warning disable CA1033 // Interface methods should be callable by child types

namespace Abanu.Tools.Build
{
    public abstract class AbanuBuilder : IBuilderEvent, IStarterEvent
    {

        public AbanuBuilder(string inputAssembly)
        {
            InputAssembly = inputAssembly;
        }

        public static PlatformType Platform
        {
            get
            {
                switch (Env.Get("ABANU_ARCH"))
                {
                    case "x86":
                        return PlatformType.x86;
                    case "x64":
                        return PlatformType.x64;
                    default:
                        return PlatformType.x86;
                }
            }
        }

        public LauncherOptions Options { get; set; }

        public string TestAssemblyPath { get; set; }

        public string InputAssembly { get; set; }
        public AppLocations AppLocations { get; set; }

        public TypeSystem TypeSystem { get; internal set; }
        public MosaLinker Linker { get; internal set; }

        protected Starter Starter;
        protected Process Process;
        protected string ImageFile;

        public abstract void Configure();

        public bool Build()
        {
            Console.WriteLine("Compile " + InputAssembly);
            Configure();

            AppLocations = new AppLocations();

            AppLocations.FindApplications();

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

        private DateTime date = DateTime.UtcNow;
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
