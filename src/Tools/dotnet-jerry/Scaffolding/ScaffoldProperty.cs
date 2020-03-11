using Jerrycurl.Tools.Scaffolding.Model;

namespace Jerrycurl.Tools.DotNet.Cli.Scaffolding
{
    public class ScaffoldProperty
    {
        public string PropertyName { get; set; }
        public string TypeName { get; set; }

        public ColumnModel Column { get; set; }
    }
}
