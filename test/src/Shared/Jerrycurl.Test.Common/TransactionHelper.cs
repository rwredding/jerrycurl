using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Jerrycurl.Data.Commands;
using Jerrycurl.Data.Filters;
using Jerrycurl.Data.Queries;
using Shouldly;

namespace Jerrycurl.Test
{
    public class TransactionHelper
    {
        public Func<IDbConnection> Factory { get; }

        private readonly string createSql;
        private readonly string insertSql;
        private readonly string selectSql;

        public TransactionHelper(Func<IDbConnection> factory, string createSql, string insertSql, string selectSql)
        {
            this.Factory = factory;
            this.createSql = createSql;
            this.insertSql = insertSql;
            this.selectSql = selectSql;
        }

        public CommandHandler GetCommandHandler(params IFilter[] filters)
        {
            return new CommandHandler(new CommandOptions()
            {
                ConnectionFactory = this.Factory,
                Filters = filters,
            });
        }

        public CommandData GetInsert() => new CommandData() { CommandText = this.insertSql };

        public void CreateTable()
        {
            CommandData command = new CommandData() { CommandText = this.createSql };
            CommandHandler handler = this.GetCommandHandler();

            handler.Execute(command);
        }

        public void VerifyTransaction() => this.SelectValues().ShouldBeEmpty();
        public void VerifyNonTransaction() => this.SelectValues().ShouldNotBeEmpty();

        public IList<int> SelectValues()
        {
            QueryOptions options = new QueryOptions()
            {
                ConnectionFactory = this.Factory,
                Schemas = DatabaseHelper.Default.Schemas,
            };

            QueryData query = new QueryData() { QueryText = this.selectSql };
            QueryHandler handler = new QueryHandler(options);

            return handler.List<int>(query);
        }
    }
}
