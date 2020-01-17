using System.Collections.Generic;

namespace Jerrycurl.Tools.Scaffolding.Model
{
    public class TableModel
    {
        public string Schema { get; set; }
        public string Name { get; set; }
        public bool Ignore { get; set; }

        public IList<ColumnModel> Columns { get; set; } = new List<ColumnModel>();
    }
}
