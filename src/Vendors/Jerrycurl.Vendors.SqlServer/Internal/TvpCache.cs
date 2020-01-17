using Jerrycurl.Data.Metadata;
using Jerrycurl.Relations;
using Jerrycurl.Relations.Metadata;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if SQLSERVER_LEGACY
using System.Data.SqlClient;
#else
using Microsoft.Data.SqlClient;
#endif


namespace Jerrycurl.Vendors.SqlServer.Internal
{
    internal static class TvpCache
    {
        public static ConcurrentDictionary<RelationIdentity, Action<SqlParameter, IRelation>> Binders { get; } = new ConcurrentDictionary<RelationIdentity, Action<SqlParameter, IRelation>>();
    }
}
