using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Jerrycurl.IO;

namespace Jerrycurl.CommandLine
{
    internal class ToolOptions : IEnumerable<ToolOption>
    {
        private readonly IReadOnlyCollection<ToolOption> options;

        public ToolOptions(IEnumerable<ToolOption> options)
        {
            this.options = options?.ToArray() ?? throw new ArgumentNullException(nameof(options));
        }

        public static string Escape(string argument)
        {
            if (argument.Any(Char.IsWhiteSpace))
                return $"\"{argument}\"";

            return argument;
        }

        public static string Escape(IEnumerable<string> args) => string.Join(" ", args.Select(Escape));

        public string[] ToArgumentList() => this.options.SelectMany(opt => opt.ToArgumentList()).ToArray();

        public static string[] ToArgumentList(string arguments)
        {
            char[] c = arguments.ToCharArray();
            bool inSingle = false;
            bool inDouble = false;

            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == '"' && !inSingle)
                {
                    inDouble = !inDouble;
                    c[i] = '\n';
                }
                if (c[i] == '\'' && !inDouble)
                {
                    inSingle = !inSingle;
                    c[i] = '\n';
                }
                if (!inSingle && !inDouble && c[i] == ' ')
                    c[i] = '\n';
            }
            return (new string(c)).Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public string[] Default => this.options.Where(opt => opt.Name == "").SelectMany(opt => opt.Values).ToArray();

        public string Escape() => Escape(this.ToArgumentList());

        public ToolOption this[params string[] options]
        {
            get
            {
                List<ToolOption> all = new List<ToolOption>();

                foreach (ToolOption opt in this.options)
                {
                    foreach (string option in options)
                    {
                        if ("--" + opt.Name == option || "-" + opt.ShortName == option)
                            all.Add(opt);
                    }
                }

                if (all.Count == 0)
                    return null;

                return new ToolOption()
                {
                    ShortName = all[0].ShortName,
                    Name = all[0].Name,
                    Values = all.SelectMany(o => o.Values).ToArray(),
                };
            }
        }

        public IEnumerator<ToolOption> GetEnumerator() => this.options.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        #region " Parsing "

        public static ToolOptions Parse(string[] args, ResponseSettings settings = null)
        {
            return new ToolOptions(ParseAndYield(args, settings));
        }

        private static IEnumerable<ToolOption> ParseAndYield(string[] args, ResponseSettings settings)
        {
            bool isDefault = true;

            List<string> newArgs = new List<string>();

            foreach (string argument in args)
            {
                if (ResponseFile.HasPathSyntax(argument, out _))
                    newArgs.AddRange(ResponseFile.ExpandStrings(argument, settings).SelectMany(ToArgumentList));
                else
                    newArgs.Add(argument);
            }

            for (int i = 0; i < newArgs.Count; i++)
            {
                if (IsOption(newArgs[i]))
                {
                    ToolOption option = CreateOption(newArgs[i]);

                    if (option != null)
                    {
                        option.Values = newArgs.Skip(i + 1).TakeWhile(s => !IsOption(s)).ToArray();

                        i += option.Values.Length;

                        option.Values = option.Values.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
                    }

                    isDefault = false;

                    yield return option;
                }
                else if (isDefault)
                {
                    ToolOption option = new ToolOption()
                    {
                        Name = "",
                        ShortName = "",
                        Values = newArgs.Skip(i).TakeWhile(s => !IsOption(s)).ToArray()
                    };

                    i += option.Values.Length - 1;

                    option.Values = option.Values.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

                    isDefault = false;

                    yield return option;
                }
            }
        }

        private static bool IsOption(string s) => s.StartsWith("-");

        private static ToolOption CreateOption(string name)
        {
            ToolOption option = new ToolOption();

            if (name.StartsWith("--"))
                option.Name = name.Remove(0, 2);
            else if (name.StartsWith("-"))
                option.ShortName = name.Remove(0, 1);
            else
                throw new InvalidOperationException($"Invalid option format '{name}'.");

            return option;
        }
        #endregion
    }
}
