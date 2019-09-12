// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Lonos.Build
{

    public class CommandArgs : IEnumerable<CommandArgument>
    {

        public static CommandArgs FromCommandlineArguments(string[] args)
        {
            var ca = new CommandArgs();
            foreach (var arg in args)
                ca.Values.Add(arg);
            return ca;
        }

        public static CommandArgs FromString(string commandLine)
        {
            if (commandLine == null)
                return new CommandArgs();

            var ar = commandLine.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return FromCommandlineArguments(ar);
        }

        public CommandArgs()
        {
        }

        public CommandArgs(CommandArgument[] args)
        {
            Values.AddRange(args);
        }

        public CommandArgs(CommandArgs arg)
        {
            Values.AddRange(arg.Values);
        }

        public List<CommandArgument> Values = new List<CommandArgument>();

        public int Count => Values.Count;

        public CommandArgument this[int argumentIndex]
        {
            get
            {
                if (argumentIndex >= Values.Count)
                    return null;

                return Values[argumentIndex];
            }
        }

        public CommandArgs Copy()
        {
            return new CommandArgs(this);
        }

        public CommandArgs Pop() => Pop(1);

        public CommandArgs Pop(int count)
        {
            var args = Copy();

            for (var i = 0; i < count; i++)
                if (args.Values.Count > 0)
                    args.Values.RemoveAt(0);

            return args;
        }

        public string[] ToStringArray()
        {
            return Values.Select(v => v.ToString()).ToArray();
        }

        public IEnumerator<CommandArgument> GetEnumerator()
        {
            return Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Values.GetEnumerator();
        }

        public override string ToString()
        {
            return string.Join(" ", ToStringArray());
        }

        public static implicit operator string(CommandArgs value) => value.ToString();
        public static implicit operator CommandArgs(string value) => FromString(value);

        public bool Contains(string value) => Values.Contains(value);

        public bool ContainsAny(params string[] value) => Values.Any(v => value.Contains(v.ToString()));

        public bool ContainsFlag(string name, string value) => ContainsAny($"--{name}={value}", $"--{value}");

    }

}
