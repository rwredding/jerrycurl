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

        public ToolOption this[string option] => this.options.FirstOrDefault(opt => "--" + opt.Name == option || "-" + opt.ShortName == option);
        public ToolOption this[params string[] options]
        {
            get
            {
                foreach (string option in options)
                {
                    ToolOption found = this[option];

                    if (found != null)
                        return found;
                }

                return null;
            }
        }

        public IEnumerator<ToolOption> GetEnumerator() => this.options.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
