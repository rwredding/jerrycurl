using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using Jerrycurl.Data.Commands;
using Jerrycurl.Data.Filters;
using Jerrycurl.Data.Queries;
using Jerrycurl.Test;
using Jerrycurl.Test.Transactions;
using Jerrycurl.Vendors.Sqlite.Test;
using Shouldly;

namespace Jerrycurl.Vendors.Sqlite.Test
{
    public class TransactionTests : TransactionTestBase
    {
        protected override Func<IDbConnection> GetConnectionFactory() => () => SqliteConvention.GetConnection();

        protected override IEnumerable<CommandData> GetEnsureTableCommands()
        {
            yield return new CommandData()
            {
                CommandText = @"CREATE TABLE IF NOT EXISTS tran_values ( Value int NOT NULL );
                                DELETE FROM tran_values;",
            };
        }
    }
}
