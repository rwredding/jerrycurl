using Jerrycurl.Data.Filters;
using System;
using System.Collections.Generic;
using System.Data;

namespace Jerrycurl.Data.Sessions
{
    public abstract class SessionOptions
    {
        public Func<IDbConnection> ConnectionFactory { get; set; }
        public ICollection<IFilter> Filters { get; set; } = new List<IFilter>();
    }
}
