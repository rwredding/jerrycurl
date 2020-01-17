using Jerrycurl.Data.Filters;
using System.Collections.Generic;

namespace Jerrycurl.Mvc
{
    public class SqlOptions : ISqlOptions
    {
        public List<IFilter> Filters { get; set; } = new List<IFilter>();

        public int MaxSql { get; set; }
        public int MaxParameters { get; set; }

        IReadOnlyCollection<IFilter> ISqlOptions.Filters => this.Filters;
    }
}
