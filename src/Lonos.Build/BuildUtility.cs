// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Lonos.Build
{
    public static class BuildUtility
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
                        value = "${LONOS_PROJDIR}/bin/x86/Lonos.Native.o";
                        break;
                    case "LONOS_BOOTLOADER_EXE":
                        value = "${LONOS_PROJDIR}/bin/Lonos.OS.Loader.x86.exe";
                        break;
                    case "LONOS_EXE":
                        value = "${LONOS_PROJDIR}/bin/Lonos.OS.Core.x86.exe";
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
    }
}
