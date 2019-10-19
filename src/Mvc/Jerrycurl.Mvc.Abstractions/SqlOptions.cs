using Jerrycurl.Data.Filters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

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
