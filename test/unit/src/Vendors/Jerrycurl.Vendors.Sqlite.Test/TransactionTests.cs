using System.Data.Common;
using System.Threading.Tasks;
using Jerrycurl.Data.Commands;
using Jerrycurl.Data.Filters;
using Jerrycurl.Test;

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

            Command command = this.helper.GetInsert();
            CommandEngine engine = this.helper.GetCommandEngine();

            
            try
            {
                engine.Execute(command);
            }
            catch (DbException) { }

            this.helper.VerifyNonTransaction();
        }

        public void Test_Inserts_WithTransaction()
        {
            this.helper.CreateTable();

            Command command = this.helper.GetInsert();
            CommandEngine engine = this.helper.GetCommandEngine(new TransactionFilter());

            try
            {
                engine.Execute(command);
            }
            catch (DbException) { }

            this.helper.VerifyTransaction();
        }

        public async Task Test_InsertsAsync_WithoutTransaction()
        {
            this.helper.CreateTable();

            Command command = this.helper.GetInsert();
            CommandEngine engine = this.helper.GetCommandEngine();

            try
            {
                await engine.ExecuteAsync(command);
            }
            catch (DbException) { }

            this.helper.VerifyNonTransaction();
        }

        public async Task Test_InsertsAsync_WithTransaction()
        {
            this.helper.CreateTable();

            Command command = this.helper.GetInsert();
            CommandEngine engine = this.helper.GetCommandEngine(new TransactionFilter());

            try
            {
                await engine.ExecuteAsync(command);
            }
            catch (DbException) { }

            this.helper.VerifyTransaction();
        }
    }
}
