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
    public abstract class TransactionScopeTestBase : TransactionSqlBase
    {
        public async Task Test_Inserts_WithAsyncTransactionScope()
        {
            this.EnsureTable();

            CommandData command = this.GetCommandData();
            CommandHandler handler = this.GetCommandHandler(new TransactionScopeFilter());

            try
            {
                await handler.ExecuteAsync(command);
            }
            catch (DbException) { }

            this.GetTableValues().ShouldBe(new[] { 1, 2 });
        }

        public void Test_Inserts_WithTransactionScope()
        {
            this.EnsureTable();

            CommandData command = this.GetCommandData();
            CommandHandler handler = this.GetCommandHandler(new TransactionScopeFilter());

            try
            {
                handler.Execute(command);
            }
            catch (DbException) { }

            this.GetTableValues().ShouldBeEmpty();
        }
    }
}
