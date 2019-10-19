namespace Jerrycurl.CommandLine
{
    internal class ToolOption
    {
        public string Name { get; set; }
        public string ShortName { get; set; }

        public string[] Values { get; set; }

        public string Value => this.Values?.Length > 0 ? this.Values[0] : null;
    }
}
