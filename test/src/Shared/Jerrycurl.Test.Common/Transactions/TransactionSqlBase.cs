using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;
using Jerrycurl.Data.Commands;
using Jerrycurl.Data.Filters;
using Jerrycurl.Data.Queries;
using Jerrycurl.Test;
using Shouldly;

namespace Jerrycurl.Test.Transactions
{
    public abstract class TransactionSqlBase
    {
        protected abstract IEnumerable<CommandData> GetEnsureTableCommands();
        protected abstract Func<IDbConnection> GetConnectionFactory();

        protected CommandData GetCommandData()
        {
            return new CommandData()
            {
                CommandText = @"INSERT INTO tran_values VALUES(1);
                                INSERT INTO tran_values VALUES(2);
                                INSERT INTO tran_values VALUES(NULL);"
            };
        }

        protected CommandHandler GetCommandHandler(params IFilter[] filters)
        {
            return new CommandHandler(new CommandOptions()
            {
                ConnectionFactory = this.GetConnectionFactory(),
                Filters = filters,
            });
        }

        protected void EnsureTable()
        {
            IEnumerable<CommandData> commands = this.GetEnsureTableCommands();
            CommandHandler handler = this.GetCommandHandler();

            handler.Execute(commands);
        }

        protected IList<int> GetTableValues()
        {
            QueryOptions options = new QueryOptions()
            {
                ConnectionFactory = this.GetConnectionFactory(),
                Schemas = DatabaseHelper.Default.Schemas,
            };

            QueryData query = new QueryData()
            {
                QueryText = "SELECT Value AS \"Item\" FROM tran_values;",
            };

            QueryHandler handler = new QueryHandler(options);

            return handler.List<int>(query);
        }
    }
}
