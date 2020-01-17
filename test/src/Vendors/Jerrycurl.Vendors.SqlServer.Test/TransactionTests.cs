using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using Jerrycurl.Data.Commands;
using Jerrycurl.Data.Filters;
using Jerrycurl.Data.Queries;
using Jerrycurl.Test;
using Microsoft.Data.SqlClient;
using Shouldly;

namespace Jerrycurl.Vendors.SqlServer.Test
{
    public class TransactionTests
    {
        public void Test_Inserts_WithTransactionScope()
        {
            this.CreateTable();
            this.ClearTable();

            CommandData command = new CommandData()
            {
                CommandText = @"INSERT INTO MyValues VALUES(1);
                                INSERT INTO MyValues VALUES(2);
                                INSERT INTO MyValues VALUES(NULL);
                                INSERT INTO MyValues VALUES(3);"
            };

            CommandOptions options = new CommandOptions()
            {
                ConnectionFactory = this.GetConnectionFactory(),
                Filters = new IFilter[]
                {
                    new TransactionScopeFilter(),
                }
            };

            CommandHandler handler = new CommandHandler(options);

            try
            {
                handler.Execute(command);
            }
            catch (DbException) { }


            this.GetCurrentValues().ShouldBeEmpty();
        }

        private Func<IDbConnection> GetConnectionFactory() => () => new SqlConnection(SqlServerConvention.GetConnectionString());

        private IList<int> GetCurrentValues()
        {
            QueryOptions options = new QueryOptions()
            {
                ConnectionFactory = this.GetConnectionFactory(),
                Schemas = DatabaseHelper.Default.Schemas,
            };

            QueryData query = new QueryData()
            {
                QueryText = "SELECT Value AS [Item] FROM MyValues",
            };

            QueryHandler handler = new QueryHandler(options);

            return handler.List<int>(query);
        }

        private void ClearTable()
        {
            CommandOptions options = new CommandOptions()
            {
                ConnectionFactory = this.GetConnectionFactory(),
            };

            CommandData command = new CommandData()
            {
                CommandText = @"DELETE FROM MyValues;",
            };

            CommandHandler handler = new CommandHandler(options);

            handler.Execute(command);
        }

        private void CreateTable()
        {
            CommandOptions options = new CommandOptions()
            {
                ConnectionFactory = this.GetConnectionFactory(),
            };

            CommandData command = new CommandData()
            {
                CommandText = @"IF NOT EXISTS(SELECT 0 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'MyValues') CREATE TABLE MyValues ( Value int NOT NULL );",
            };

            CommandHandler handler = new CommandHandler(options);

            handler.Execute(command);
        }
    }
}
