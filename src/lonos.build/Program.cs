// Copyright (c) MOSA Project. Licensed under the New BSD License.

using System;
using System.Text.RegularExpressions;
using System.IO;

namespace lonos.build
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
                    case "LONOS_PROJECT_ROOT":
                        value = Path.GetDirectoryName( Path.GetDirectoryName(new Uri(typeof(Program).Assembly.Location).AbsolutePath));
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

            file = GetEnv("LONOS_EXE");

            var builder = new LonosBuilder(file);
            builder.Build();
            System.Console.WriteLine("ready");
            //System.Console.ReadLine();
        }
    }
}
