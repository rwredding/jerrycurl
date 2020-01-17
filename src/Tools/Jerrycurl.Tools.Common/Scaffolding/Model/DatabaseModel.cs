using System.Collections.Generic;

namespace Jerrycurl.Tools.Scaffolding.Model
{
    public class DatabaseModel
    {
        public IList<TableModel> Tables { get; set; } = new List<TableModel>();
        public string DefaultSchema { get; set; }
    }
}
