using System.Data.Common;
using System.Threading.Tasks;
using Jerrycurl.Data.Commands;
using Jerrycurl.Data.Filters;
using Jerrycurl.Test;

namespace Jerrycurl.Vendors.Oracle.Test
{
    public class TransactionTests
    {
        private readonly TransactionHelper helper = new TransactionHelper(() => OracleConvention.GetConnection(), CreateSql, InsertSql, SelectSql);

        private const string InsertSql = @"BEGIN
                                                INSERT INTO ""tran_values"" VALUES(1);
                                                INSERT INTO ""tran_values"" VALUES(2);
                                                INSERT INTO ""tran_values"" VALUES(NULL);
                                           END;";
        private const string SelectSql = @"SELECT ""v"" AS ""Item"" FROM ""tran_values""";
        private const string CreateSql = @"BEGIN
                                              BEGIN
                                                  EXECUTE IMMEDIATE 'DROP TABLE ""tran_values""';
                                              EXCEPTION
                                                  WHEN OTHERS THEN NULL;
                                              END;
                                              BEGIN
                                                  EXECUTE IMMEDIATE 'CREATE TABLE ""tran_values"" ( ""v"" NUMBER NOT NULL )';
                                              EXCEPTION
                                                  WHEN OTHERS THEN NULL;
                                              END;
                                           END;";

        public void Test_Inserts_WithImplicitTransaction()
        {
            this.helper.CreateTable();

            Command command = this.helper.GetInsert();
            CommandEngine engine = this.helper.GetCommandEngine();

            try
            {
                engine.Execute(command);
            }
            catch (DbException) { }

            this.helper.VerifyTransaction();
        }

        public async Task Test_InsertsAsync_WithImplicitTransaction()
        {
            this.helper.CreateTable();

            Command command = this.helper.GetInsert();
            CommandEngine engine = this.helper.GetCommandEngine();

            try
            {
                await engine.ExecuteAsync(command);
            }
            catch (DbException) { }

            this.helper.VerifyTransaction();
        }


        public void Test_MultiInsert_WithoutExplicitTransaction()
        {
            this.helper.CreateTable();

            Command[] commands = this.GetMultiInsertCommands();
            CommandEngine engine = this.helper.GetCommandEngine();

            try
            {
                engine.Execute(commands);
            }
            catch (DbException) { }

            this.helper.VerifyNonTransaction();
        }

        public async Task Test_MultiInsertAsync_WithoutExplicitTransaction()
        {
            this.helper.CreateTable();

            Command[] commands = this.GetMultiInsertCommands();
            CommandEngine engine = this.helper.GetCommandEngine();

            try
            {
                await engine.ExecuteAsync(commands);
            }
            catch (DbException) { }

            this.helper.VerifyNonTransaction();
        }

        public void Test_MultiInsert_WithExplicitTransaction()
        {
            this.helper.CreateTable();

            Command[] commands = this.GetMultiInsertCommands();
            CommandEngine engine = this.helper.GetCommandEngine(new TransactionFilter());

            try
            {
                engine.Execute(commands);
            }
            catch (DbException) { }

            this.helper.VerifyTransaction();
        }

        public async Task Test_MultiInsertAsync_WithExplicitTransaction()
        {
            this.helper.CreateTable();

            Command[] commands = this.GetMultiInsertCommands();
            CommandEngine engine = this.helper.GetCommandEngine(new TransactionFilter());

            try
            {
                await engine.ExecuteAsync(commands);
            }
            catch (DbException) { }

            this.helper.VerifyTransaction();
        }

        public async Task Test_MultiInsertAsync_WithExplicitTransactionScope()
        {
            this.helper.CreateTable();

            Command[] commands = this.GetMultiInsertCommands();
            CommandEngine engine = this.helper.GetCommandEngine(new TransactionScopeFilter());

            try
            {
                await engine.ExecuteAsync(commands);
            }
            catch (DbException) { }

            this.helper.VerifyTransaction();
        }

        public void Test_MultiInsert_WithExplicitTransactionScope()
        {
            this.helper.CreateTable();

            Command[] commands = this.GetMultiInsertCommands();
            CommandEngine engine = this.helper.GetCommandEngine(new TransactionScopeFilter());

            try
            {
                engine.Execute(commands);
            }
            catch (DbException) { }

            this.helper.VerifyTransaction();
        }

        private Command[] GetMultiInsertCommands()
        {
            return new Command[]
            {
                new Command()
                {
                    CommandText = @"BEGIN
                                        INSERT INTO ""tran_values"" VALUES(1);
                                        INSERT INTO ""tran_values"" VALUES(2);
                                    END;",
                },
                new Command()
                {
                    CommandText = @"BEGIN
                                        INSERT INTO ""tran_values"" VALUES(NULL);
                                    END;",
                },
            };
        }
    }
}
