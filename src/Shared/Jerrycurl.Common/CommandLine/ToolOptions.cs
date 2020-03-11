using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

        public string[] ToArgumentList()
        {
            return this.options.SelectMany(opt => new[] { opt.Name != null ? "--" + opt.Name : "-" + opt.ShortName }.Concat(opt.Values)).ToArray();
        }

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

                            ToolOption found = this[option];

                        if (found != null)
                            return found;
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
    }
}
