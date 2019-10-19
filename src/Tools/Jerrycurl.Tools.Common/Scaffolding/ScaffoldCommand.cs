using Jerrycurl.Tools.Scaffolding.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jerrycurl.Tools.Scaffolding
{
    public abstract class ScaffoldCommand : IConnectionFactory
    {
        public abstract DbConnection GetDbConnection();
        public abstract Task<DatabaseModel> GetDatabaseModelAsync(DbConnection connection, CancellationToken cancellationToken = default);
        public abstract IEnumerable<TypeMapping> GetTypeMappings();
    }
}