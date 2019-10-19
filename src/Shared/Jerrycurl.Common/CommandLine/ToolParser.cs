using System;
using System.Collections.Generic;
using System.Linq;

namespace Jerrycurl.CommandLine
{
    internal class ToolParser
    {
        public ToolOptions Parse(string[] args)
        {
            return new ToolOptions(this.ParseAndYield(args));
        }

        private IEnumerable<ToolOption> ParseAndYield(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (this.IsOption(args[i]))
                {
                    ToolOption option = this.CreateOption(args[i]);

                    if (option != null)
                    {
                        option.Values = args.Skip(i + 1).TakeWhile(s => !this.IsOption(s)).ToArray();

                        i += option.Values.Length;
                    }

                    yield return option;
                }
            }
        }

        private bool IsOption(string s)
        {
            return s.StartsWith("-");
        }

        private ToolOption CreateOption(string name)
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
    }
}
