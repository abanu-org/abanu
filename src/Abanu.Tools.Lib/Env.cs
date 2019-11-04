// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace Abanu.Tools
{

    public static class Env
    {

        public static CultureInfo LocaleInvariant = CultureInfo.InvariantCulture;

        public static void Set(string name, string value)
        {
            Vars.Remove(name);
            Vars.Add(name, value);
        }

        private static Dictionary<string, string> Vars = new Dictionary<string, string>();

        public static string Get(string name)
        {
            var value = Environment.GetEnvironmentVariable(name);
            if (value == null)
            {
                switch (name)
                {
                    case "ABANU_PROJDIR":
                        value = Path.GetDirectoryName(Path.GetDirectoryName(new Uri(typeof(Env).Assembly.Location).AbsolutePath));
                        if (!File.Exists(Path.Combine(value, "abctl")))
                            value = Path.GetDirectoryName(value);
                        if (!File.Exists(Path.Combine(value, "abctl")))
                            value = Path.GetDirectoryName(value);
                        if (!File.Exists(Path.Combine(value, "abctl")))
                            value = Path.GetDirectoryName(value);
                        if (!File.Exists(Path.Combine(value, "abctl")))
                            value = null;
                        break;
                    case "ABANU_ARCH":
                        value = "x86";
                        break;
                    case "ABANU_BINDIR":
                        value = "${ABANU_PROJDIR}/bin";
                        break;
                    case "ABANU_OSDIR":
                        value = "${ABANU_PROJDIR}/os";
                        break;
                    case "ABANU_NATIVE_FILES":
                        value = "${ABANU_PROJDIR}/bin/${ABANU_ARCH}/Abanu.Native.o";
                        break;
                    case "ABANU_BOOTLOADER_EXE":
                        value = "${ABANU_PROJDIR}/bin/Abanu.OS.Loader.${ABANU_ARCH}.exe";
                        break;
                    case "ABANU_EXE":
                        value = "${ABANU_PROJDIR}/bin/Abanu.OS.Core.${ABANU_ARCH}.exe";
                        break;
                    case "ABANU_LOGDIR":
                        value = "${ABANU_PROJDIR}/logs";
                        break;
                    case "ABANU_ISODIR":
                        value = "${ABANU_PROJDIR}/iso";
                        break;
                    case "ABANU_TOOLSDIR":
                        value = "${ABANU_PROJDIR}/tools";
                        break;
                    case "MOSA_PROJDIR":
                        value = "${ABANU_PROJDIR}/external/MOSA-Project";
                        break;
                    case "MOSA_TOOLSDIR":
                        value = "${MOSA_PROJDIR}/Tools";
                        break;
                    case "qemu":
                        if (Environment.OSVersion.Platform == PlatformID.Unix)
                            value = "qemu-system-x86_64";
                        else
                            value = "${MOSA_TOOLSDIR}/qemu/qemu-system-x86_64.exe";
                        break;
                    case "gdb":
                        value = @"gdb.exe";
                        break;
                    case "nasm":
                        value = "${MOSA_TOOLSDIR}/nasm/nasm.exe";
                        break;
                }

                if (Vars.ContainsKey(name))
                    value = Vars[name];
            }

            var regex = new Regex(@"\$\{(\w+)\}", RegexOptions.RightToLeft);

            if (value == null)
                value = name;

            if (value != null)
                foreach (Match m in regex.Matches(value))
                    value = value.Replace(m.Value, Get(m.Groups[1].Value));
            return value;
        }
    }
}
