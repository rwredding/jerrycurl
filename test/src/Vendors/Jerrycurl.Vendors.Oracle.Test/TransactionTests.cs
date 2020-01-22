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
using Shouldly;

namespace Jerrycurl.Vendors.Oracle.Test
{
    public class TransactionTests : TransactionTestBase
    {
        protected override Func<IDbConnection> GetConnectionFactory() => () => OracleConvention.GetConnection();

        protected override IEnumerable<CommandData> GetEnsureTableCommands()
        {
            yield return new CommandData()
            {
                CommandText = @"BEGIN
                                    EXECUTE IMMEDIATE 'DROP TABLE ""tran_values""';
                                EXCEPTION
                                   WHEN OTHERS THEN
                                      NULL;
                                END;"
            };

            yield return new CommandData()
            {
                CommandText = @"CREATE TABLE ""tran_values"" ( Value int NOT NULL );"
            };
        }
    }
}
