using System;
using System.Threading.Tasks;
using Jerrycurl.Data.Commands;
using Microsoft.Data.Sqlite;
using Shouldly;
using Jerrycurl.Data.Queries;
using Jerrycurl.Test;
using System.Data;
using System.Threading;
using Jerrycurl.Data.Sessions;

namespace Jerrycurl.Data.Test
{
    public class SessionTests
    {
        public async Task Test_ConnectionManagement_WithAsyncSession()
        {
            var connection = new SqliteConnection(DatabaseHelper.TestDbConnectionString);

            QueryOptions options = new QueryOptions()
            {
                ConnectionFactory = () => connection,
                Schemas = DatabaseHelper.Default.Schemas,
            };

            await using (var ado = new AsyncSession(options.ConnectionFactory, null))
            {
                await foreach (var r in ado.ExecuteAsync(new SqlOperation("SELECT 12; SELECT 12"), CancellationToken.None))
                {
                    (await r.ReadAsync()).ShouldBeTrue();
                    (await r.GetFieldValueAsync<int>(0)).ShouldBe(12);
                    (await r.ReadAsync()).ShouldBeFalse();
                }
            }

            connection.State.ShouldBe(ConnectionState.Closed);
        }

        public void Test_ConnectionManagement_WithSyncSession()
        {
            var connection = new SqliteConnection(DatabaseHelper.TestDbConnectionString);

            QueryOptions options = new QueryOptions()
            {
                ConnectionFactory = () => connection,
                Schemas = DatabaseHelper.Default.Schemas,
            };

            using (var ado = new SyncSession(options.ConnectionFactory, options.Filters))
            {
                foreach (var r in ado.Execute(new SqlOperation("SELECT 12; SELECT 12")))
                {
                    r.Read().ShouldBeTrue();
                    r.GetInt32(0).ShouldBe(12);
                    r.Read().ShouldBeFalse();
                }
            }

            connection.State.ShouldBe(ConnectionState.Closed);
        }

        public void Test_ConnectionManagement_WithOpenConnection()
        {
            var connection = new SqliteConnection(DatabaseHelper.TestDbConnectionString);

            connection.Open();

            QueryOptions options = new QueryOptions()
            {
                ConnectionFactory = () => connection,
                Schemas = DatabaseHelper.Default.Schemas,
            };

            Should.Throw<InvalidOperationException>(() =>
            {
                try
                {
                    new SyncSession(options.ConnectionFactory, options.Filters);
                }
                finally
                {
                    connection.Dispose();
                }
            });
        }

        public async Task Test_ConnectionManagement_WithCommandHandler()
        {
            var connection1 = new SqliteConnection(DatabaseHelper.TestDbConnectionString);
            var connection2 = new SqliteConnection(DatabaseHelper.TestDbConnectionString);

            try
            {
                CommandOptions options1 = new CommandOptions()
                {
                    ConnectionFactory = () => connection1,
                };

                CommandOptions options2 = new CommandOptions()
                {
                    ConnectionFactory = () => connection2,
                };

                CommandEngine engine1 = new CommandEngine(options1);
                CommandEngine engine2 = new CommandEngine(options2);

                engine1.Execute(new Command() { CommandText = "SELECT 0;" });
                await engine2.ExecuteAsync(new Command() { CommandText = "SELECT 0;" });

                connection1.State.ShouldBe(ConnectionState.Closed);
                connection2.State.ShouldBe(ConnectionState.Closed);
            }
            finally
            {
                connection1.Dispose();
                connection2.Dispose();
            }
        }


        public async Task Test_ConnectionManagement_WithQueryHandler()
        {
            var connection1 = new SqliteConnection(DatabaseHelper.TestDbConnectionString);
            var connection2 = new SqliteConnection(DatabaseHelper.TestDbConnectionString);

            try
            {
                QueryOptions options1 = new QueryOptions()
                {
                    ConnectionFactory = () => connection1,
                    Schemas = DatabaseHelper.Default.Schemas,
                };

                QueryOptions options2 = new QueryOptions()
                {
                    ConnectionFactory = () => connection2,
                    Schemas = DatabaseHelper.Default.Schemas,
                };

                QueryEngine handler1 = new QueryEngine(options1);
                QueryEngine handler2 = new QueryEngine(options2);

                handler1.List<int>(new Query() { QueryText = "SELECT 0 AS [Item];" });
                await handler2.ListAsync<int>(new Query() { QueryText = "SELECT 0 AS [Item];" });

                connection1.State.ShouldBe(ConnectionState.Closed);
                connection2.State.ShouldBe(ConnectionState.Closed);
            }
            finally
            {
                connection1.Dispose();
                connection2.Dispose();
            }
        }
    }
}
