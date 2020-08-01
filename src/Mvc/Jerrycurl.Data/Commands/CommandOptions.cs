using System;
using System.Collections.Generic;
using System.Data;
using Jerrycurl.Data.Filters;
using Jerrycurl.Data.Sessions;

namespace Jerrycurl.Data.Commands
{
    public class CommandOptions : ISessionOptions
    {
        public Func<IDbConnection> ConnectionFactory { get; set; }
        public ICollection<IFilter> Filters { get; set; } = new List<IFilter>();

        public virtual IAsyncSession GetAsyncSession() => new AsyncSession(this.ConnectionFactory, this.Filters);
        public virtual ISyncSession GetSyncSession() => new SyncSession(this.ConnectionFactory, this.Filters);
    }
}
