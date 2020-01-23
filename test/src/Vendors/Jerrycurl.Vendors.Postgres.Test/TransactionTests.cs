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

namespace Jerrycurl.Vendors.Postgres.Test
{
    public class TransactionTests
    {
        private readonly TransactionHelper helper = new TransactionHelper(() => PostgresConvention.GetConnection(), CreateSql, InsertSql, SelectSql);

        private const string InsertSql = @"INSERT INTO tran_values VALUES(1);
                                           INSERT INTO tran_values VALUES(2);
                                           INSERT INTO tran_values VALUES(NULL);";
        private const string SelectSql = @"SELECT v AS ""Item"" FROM tran_values";
        private const string CreateSql = @"CREATE TABLE IF NOT EXISTS tran_values (v int NOT NULL );
                                           DELETE FROM tran_values;";

        public void Test_Inserts_WithImplicitTransaction()
        {
            this.helper.CreateTable();

            CommandData command = this.helper.GetInsert();
            CommandHandler handler = this.helper.GetCommandHandler();

            try
            {
                handler.Execute(command);
            }
            catch (DbException) { }

            this.helper.VerifyTransaction();
        }

        public async Task Test_InsertsAsync_WithImplicitTransaction()
        {
            this.helper.CreateTable();

            CommandData command = this.helper.GetInsert();
            CommandHandler handler = this.helper.GetCommandHandler();

            try
            {
                await handler.ExecuteAsync(command);
            }
            catch (DbException) { }

            this.helper.VerifyTransaction();
        }

        public void Test_MultiInsert_WithoutExplicitTransaction()
        {
            this.helper.CreateTable();

            CommandData[] commands = this.GetMultiInsertCommands();
            CommandHandler handler = this.helper.GetCommandHandler();

            try
            {
                handler.Execute(commands);
            }
            catch (DbException) { }

            this.helper.VerifyNonTransaction();
        }

        public async Task Test_MultiInsertAsync_WithoutExplicitTransaction()
        {
            this.helper.CreateTable();

            CommandData[] commands = this.GetMultiInsertCommands();
            CommandHandler handler = this.helper.GetCommandHandler();

            try
            {
                await handler.ExecuteAsync(commands);
            }
            catch (DbException) { }

            this.helper.VerifyNonTransaction();
        }

        public void Test_MultiInsert_WithExplicitTransaction()
        {
            this.helper.CreateTable();

            CommandData[] commands = this.GetMultiInsertCommands();
            CommandHandler handler = this.helper.GetCommandHandler(new TransactionFilter());

            try
            {
                handler.Execute(commands);
            }
            catch (DbException) { }

            this.helper.VerifyTransaction();
        }

        public async Task Test_MultiInsertAsync_WithExplicitTransaction()
        {
            this.helper.CreateTable();

            CommandData[] commands = this.GetMultiInsertCommands();
            CommandHandler handler = this.helper.GetCommandHandler(new TransactionFilter());

            try
            {
                await handler.ExecuteAsync(commands);
            }
            catch (DbException) { }

            this.helper.VerifyTransaction();
        }

        public async Task Test_MultiInsertAsync_WithExplicitTransactionScope()
        {
            this.helper.CreateTable();

            CommandData[] commands = this.GetMultiInsertCommands();
            CommandHandler handler = this.helper.GetCommandHandler(new TransactionScopeFilter());

            try
            {
                await handler.ExecuteAsync(commands);
            }
            catch (DbException) { }

            this.helper.VerifyTransaction();
        }

        public void Test_MultiInsert_WithExplicitTransactionScope()
        {
            this.helper.CreateTable();

            CommandData[] commands = this.GetMultiInsertCommands();
            CommandHandler handler = this.helper.GetCommandHandler(new TransactionScopeFilter());

            try
            {
                handler.Execute(commands);
            }
            catch (DbException) { }

            this.helper.VerifyTransaction();
        }

        private CommandData[] GetMultiInsertCommands()
        {
            return new CommandData[]
            {
                new CommandData() { CommandText = "INSERT INTO tran_values VALUES(1); INSERT INTO tran_values VALUES(2);" },
                new CommandData() { CommandText = "INSERT INTO tran_values VALUES(NULL);" },
            };
        }
    }
}
