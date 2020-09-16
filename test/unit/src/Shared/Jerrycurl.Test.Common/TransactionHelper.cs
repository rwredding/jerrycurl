using System;
using System.Collections.Generic;
using System.Data;
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

        public CommandEngine GetCommandEngine(params IFilter[] filters)
        {
            return new CommandEngine(new CommandOptions()
            {
                ConnectionFactory = this.Factory,
                Filters = filters,
            });
        }

        public Command GetInsert() => new Command() { CommandText = this.insertSql };

        public void CreateTable()
        {
            Command command = new Command() { CommandText = this.createSql };
            CommandEngine handler = this.GetCommandEngine();

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

            Query query = new Query() { QueryText = this.selectSql };
            QueryEngine handler = new QueryEngine(options);

            return handler.List<int>(query);
        }
    }
}
