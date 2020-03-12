namespace Jerrycurl.CommandLine
{
    internal class ToolOption
    {
        public string Name { get; set; }
        public string ShortName { get; set; }

        public string[] Values { get; set; }

        public string Value => this.Values?.Length > 0 ? this.Values[0] : null;

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(this.Name))
                return "--" + this.Name;
            else if (!string.IsNullOrEmpty(this.ShortName))
                return "-" + this.ShortName;
            else
                return this.Value;
        }
    }
}
