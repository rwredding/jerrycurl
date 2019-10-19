using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jerrycurl.Tools.Scaffolding.Model
{
    public class DatabaseModel
    {
        public IList<TableModel> Tables { get; set; } = new List<TableModel>();
        public string DefaultSchema { get; set; }
    }
}
