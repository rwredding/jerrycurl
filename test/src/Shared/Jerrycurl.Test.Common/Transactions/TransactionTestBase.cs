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
    public abstract class TransactionTestBase : TransactionSqlBase
    {
        public void Test_Inserts_WithoutTransaction()
        {
            this.EnsureTable();

            CommandData command = this.GetCommandData();
            CommandHandler handler = this.GetCommandHandler();

            try
            {
                handler.Execute(command);
            }
            catch (DbException) { }

            this.GetTableValues().ShouldBe(new[] { 1, 2 });
        }

        public void Test_Inserts_WithTransaction()
        {
            this.EnsureTable();

            CommandData command = this.GetCommandData();
            CommandHandler handler = this.GetCommandHandler(new TransactionFilter());

            try
            {
                handler.Execute(command);
            }
            catch (DbException) { }

            this.GetTableValues().ShouldBeEmpty();
        }

        public async Task Test_Inserts_WithoutAsyncTransaction()
        {
            this.EnsureTable();

            CommandData command = this.GetCommandData();
            CommandHandler handler = this.GetCommandHandler();

            try
            {
                await handler.ExecuteAsync(command);
            }
            catch (DbException) { }

            this.GetTableValues().ShouldBe(new[] { 1, 2 });
        }

        public async Task Test_Inserts_WithAsyncTransaction()
        {
            this.EnsureTable();

            CommandData command = this.GetCommandData();
            CommandHandler handler = this.GetCommandHandler(new TransactionFilter());

            try
            {
                await handler.ExecuteAsync(command);
            }
            catch (DbException) { }

            this.GetTableValues().ShouldBeEmpty();
        }
    }
}
