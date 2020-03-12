using System.Collections.Generic;
using Jerrycurl.Tools.Scaffolding.Model;

namespace Jerrycurl.Tools.DotNet.Cli.Scaffolding
{
    internal class ScaffoldObject
    {
        public string Namespace { get; set; }
        public string ClassName { get; set; }

        public TableModel Table { get; set; }
        public IList<ScaffoldProperty> Properties { get; set; } = new List<ScaffoldProperty>();
    }
}
