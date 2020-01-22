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
using Microsoft.Data.SqlClient;
using Shouldly;

namespace Jerrycurl.Vendors.SqlServer.Test
{
    public class TransactionScopeTests : TransactionScopeTestBase
    {
        protected override Func<IDbConnection> GetConnectionFactory() => () => SqlServerConvention.GetConnection();

        protected override IEnumerable<CommandData> GetEnsureTableCommands()
        {
            yield return new CommandData()
            {
                CommandText = @"IF NOT EXISTS(SELECT 0 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tran_values')
                                    CREATE TABLE tran_values ( Value int NOT NULL );
                                TRUNCATE TABLE tran_values;",
            };
        }
    }
}
