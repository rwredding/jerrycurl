using System.Collections.Generic;
using Jerrycurl.Data.Sessions;

namespace Jerrycurl.Data.Queries
{
    public class QueryData
    {
        public string QueryText { get; set; }
        public ICollection<IParameter> Parameters { get; set; } = new List<IParameter>();
    }
}
