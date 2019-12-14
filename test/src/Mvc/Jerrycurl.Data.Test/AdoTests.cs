using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jerrycurl.Data;
using Jerrycurl.Relations.Metadata;
using Jerrycurl.Relations;
using Jerrycurl.Data.Commands;
using Microsoft.Data.Sqlite;
using Shouldly;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Data.Queries;
using Jerrycurl.Data.Test.Models;
using Jerrycurl.Test;
using System.Data;
using System.Threading;

namespace Jerrycurl.Data.Test
{
    public class AdoTests
    {
#if NETCOREAPP3_0
        public async Task Test_ConnectionManagement_WithAsync()
        {
            var connection = new SqliteConnection(DatabaseHelper.TestDbConnectionString);

            await connection.OpenAsync();

            QueryOptions options = new QueryOptions()
            {
                ConnectionFactory = () => connection,
                Schemas = DatabaseHelper.Default.Schemas,
            };

            await using (var ado = new AdoConnection(options))
            {
                await foreach (var r in ado.ExecuteAsync(new AdoCommandBuilder("SELECT 12; SELECT 12"), CancellationToken.None))
                {
                    (await r.ReadAsync()).ShouldBeTrue();
                    (await r.GetFieldValueAsync<int>(0)).ShouldBe(12);
                    (await r.ReadAsync()).ShouldBeFalse();
                }
            }

            connection.State.ShouldBe(ConnectionState.Closed);
        }
#endif
        public void Test_ConnectionManagement_WithSharedFactory()
        {
            var connection = new SqliteConnection(DatabaseHelper.TestDbConnectionString);

            connection.Open();

            QueryOptions options = new QueryOptions()
            {
                ConnectionFactory = () => connection,
                Schemas = DatabaseHelper.Default.Schemas,
            };

            using (var ado = new AdoConnection(options))
            {
                foreach (var r in ado.Execute(new AdoCommandBuilder("SELECT 12; SELECT 12")))
                {
                    r.Read().ShouldBeTrue();
                    r.GetInt32(0).ShouldBe(12);
                    r.Read().ShouldBeFalse();
                }

                foreach (var r in ado.Execute(new AdoCommandBuilder("SELECT 12; SELECT 12")))
                {
                    r.Read().ShouldBeTrue();
                    r.GetInt32(0).ShouldBe(12);
                    r.Read().ShouldBeFalse();
                }
            }

            connection.State.ShouldBe(ConnectionState.Closed);
        }

        public void Test_ConnectionManagement_WithTransientFactory()
        {
            bool wasCreated = false;

            QueryOptions options = new QueryOptions()
            {
                ConnectionFactory = () =>
                {
                    if (!wasCreated)
                    {
                        wasCreated = true;

                        return new SqliteConnection(DatabaseHelper.TestDbConnectionString);
                    }

                    throw new InvalidOperationException();
                },
                Schemas = DatabaseHelper.Default.Schemas,
            };

            var ado = new AdoConnection(options);

            using (ado)
            {
                foreach (var r in ado.Execute(new AdoCommandBuilder("SELECT 12; SELECT 12")))
                {
                    r.Read().ShouldBeTrue();
                    r.GetInt32(0).ShouldBe(12);
                    r.Read().ShouldBeFalse();
                }

                foreach (var r in ado.Execute(new AdoCommandBuilder("SELECT 12; SELECT 12")))
                {
                    r.Read().ShouldBeTrue();
                    r.GetInt32(0).ShouldBe(12);
                    r.Read().ShouldBeFalse();
                }
            }

            Should.Throw<AdoException>(() =>
            {
                foreach (var r in ado.Execute(new AdoCommandBuilder("SELECT 12; SELECT 12")))
                    ;
            });

            Should.Throw<AdoException>(() =>
            {
                var ado2 = new AdoConnection(options);

                foreach (var r in ado2.Execute(new AdoCommandBuilder("SELECT 12; SELECT 12")))
                    ;
            });
        }
    }
}
