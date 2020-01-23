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
using Jerrycurl.Vendors.Sqlite.Test;
using Shouldly;

namespace Jerrycurl.Vendors.Sqlite.Test
{
    public class TransactionTests
    {
        private readonly TransactionHelper helper = new TransactionHelper(() => SqliteConvention.GetConnection(), CreateSql, InsertSql, SelectSql);

        private const string InsertSql = @"INSERT INTO tran_values VALUES(1);
                                           INSERT INTO tran_values VALUES(2);
                                           INSERT INTO tran_values VALUES(NULL);";
        private const string SelectSql = @"SELECT v AS ""Item"" FROM tran_values";
        private const string CreateSql = @"DROP TABLE IF EXISTS tran_values;
                                           CREATE TABLE tran_values ( v int NOT NULL );";



        public void Test_Inserts_WithoutTransaction()
        {
            this.helper.CreateTable();

            CommandData command = this.helper.GetInsert();
            CommandHandler handler = this.helper.GetCommandHandler();

            
            try
            {
                handler.Execute(command);
            }
            catch (DbException) { }

            this.helper.VerifyNonTransaction();
        }

        public void Test_Inserts_WithTransaction()
        {
            this.helper.CreateTable();

            CommandData command = this.helper.GetInsert();
            CommandHandler handler = this.helper.GetCommandHandler(new TransactionFilter());

            try
            {
                handler.Execute(command);
            }
            catch (DbException) { }

            this.helper.VerifyTransaction();
        }

        public async Task Test_InsertsAsync_WithoutTransaction()
        {
            this.helper.CreateTable();

            CommandData command = this.helper.GetInsert();
            CommandHandler handler = this.helper.GetCommandHandler();

            try
            {
                await handler.ExecuteAsync(command);
            }
            catch (DbException) { }

            this.helper.VerifyNonTransaction();
        }

        public async Task Test_InsertsAsync_WithTransaction()
        {
            this.helper.CreateTable();

            CommandData command = this.helper.GetInsert();
            CommandHandler handler = this.helper.GetCommandHandler(new TransactionFilter());

            try
            {
                await handler.ExecuteAsync(command);
            }
            catch (DbException) { }

            this.helper.VerifyTransaction();
        }
    }
}
