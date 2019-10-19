using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jerrycurl.Data;
using Jerrycurl.Data.Filters;

namespace Jerrycurl.Data.Queries
{
    public class QueryData
    {
        public string QueryText { get; set; }
        public ICollection<IParameter> Parameters { get; set; } = new List<IParameter>();
    }
}
