// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;

namespace Lonos.Tools.Build
{

    public struct CommandArgument
    {
        public CommandArgument(string value)
        {
            Value = value;
        }

        public CommandArgument(CommandArgument arg)
        {
            Value = arg.Value;
        }

        public string Value { get; set; }

        public static implicit operator string(CommandArgument value) => value.ToString();
        public static implicit operator CommandArgument(string value) => FromString(value);

        public override string ToString()
        {
            return Convert.ToString(Value ?? "", Env.LocaleInvariant);
        }

        public static CommandArgument FromString(string value) => new CommandArgument(value);

        public CommandArgument Copy()
        {
            return new CommandArgument(this);
        }

    }

}
