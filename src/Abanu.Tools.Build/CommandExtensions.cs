// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System.Linq;

namespace Abanu.Tools.Build
{
    public static class CommandExtensions
    {

        public static bool IsSet(this CommandArgument arg)
        {
            return arg != null && arg.Value != null && arg.ToString().Length > 0;
        }

        public static bool IsSet(this CommandArgs args)
        {
            return args != null && args.Count > 0;
        }

        public static string GetEnv(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return "";
            return Env.Get(str);
        }

        public static string GetEnv(this CommandArgument arg)
        {
            if (!arg.IsSet())
                return "";
            return Env.Get(arg);
        }

        public static CommandArgs GetEnv(this CommandArgs args)
        {
            return new CommandArgs(args.Select(arg => new CommandArgument(arg.GetEnv())).ToArray());
        }

        public static void WaitForIdle(this ProcessResult result) => result?.Process?.WaitForInputIdle();
        public static void WaitForExit(this ProcessResult result) => result?.Process?.WaitForExit();

    }

}
