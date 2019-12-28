using Jerrycurl.Data;
using Jerrycurl.Data.Filters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jerrycurl.Data.Ado
{
    public class ModernConnection : IAsyncDisposable
    {
        private readonly IDbConnection connectionBase;
        private readonly DbConnection connection;
        private readonly (IFilterHandler, IFilterAsyncHandler)[] filters;

        private bool wasDisposed = false;
        private bool wasOpened = false;

        public ModernConnection(AdoOptions options)
        {
            this.connectionBase = options?.ConnectionFactory?.Invoke();
            this.connection = this.connectionBase as DbConnection;
            this.filters = options.Filters.Select(f => (f.GetHandler(), f.GetAsyncHandler())).ToArray();

            this.VerifyAsync();
        }

        private void VerifyAsync()
        {
            if (this.connectionBase == null)
                throw new AdoException("No connection returned.");

            if (this.connection == null)
                throw new AdoException("Async operations are only available for DbConnection instances.");
        }

        public async IAsyncEnumerable<DbDataReader> ExecuteAsync(IAdoCommandBuilder builder, [EnumeratorCancellation]CancellationToken cancellationToken)
        {
            DbConnection connection = await this.GetOpenConnectionAsync(cancellationToken).ConfigureAwait(false);

            using DbCommand dbCommand = connection.CreateCommand();

            try
            {
                builder.Build(dbCommand);
            }
            catch (Exception ex)
            {
                this.ApplyCommandFilters(h => h.OnCommandException, dbCommand, ex);

                throw new AdoException("An error occurred building the ADO.NET command object.", ex);
            }

            this.ApplyCommandFilters(h => h.OnCommandCreated, dbCommand);

            DbDataReader reader = null;

            try
            {
                reader = await dbCommand.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                this.ApplyCommandFilters(h => h.OnCommandException, dbCommand, ex);

                throw new AdoException("An error occurred executing the ADO.NET command.", ex);
            }

            try
            {
                do yield return reader;
                while (await reader.NextResultAsync(cancellationToken).ConfigureAwait(false));
            }
            finally
            {
                reader?.Dispose();
            }

            this.ApplyCommandFilters(h => h.OnCommandExecuted, dbCommand);
        }

        private async Task<DbConnection> GetOpenConnectionAsync(CancellationToken cancellationToken)
        {
            if (this.wasDisposed)
                throw new AdoException("Connection is disposed.");

            if (this.connection.State == ConnectionState.Open)
                return this.connection;

            if (this.connection.State == ConnectionState.Broken)
                await this.connection.CloseAsync();

            try
            {
                if (!this.wasOpened)
                    this.ApplyConnectionFilters(h => h.OnConnectionOpening);

                if (this.connection.State == ConnectionState.Closed)
                    await this.connection.OpenAsync(cancellationToken).ConfigureAwait(false);

                if (!this.wasOpened)
                    this.ApplyConnectionFilters(h => h.OnConnectionOpened);
            }
            finally
            {
                this.wasOpened = true;
            }

            return this.connection;
        }

        private void ApplyCommandFilters(Func<IFilterHandler, Action<FilterContext>> action, IDbCommand command, Exception exception = null)
            => this.ApplyFilters(h => action(h)(new FilterContext(command, exception)));

        private void ApplyConnectionFilters(Func<IFilterHandler, Action<FilterContext>> action, Exception exception = null)
            => this.ApplyFilters(h => action(h)(new FilterContext(this.connection, exception)));

        private void ApplyFilters(Action<IFilterHandler> action)
        {
            foreach (IFilterHandler filter in this.filters)
                action(filter);
        }

        private async Task ApplyFiltersAsync(Func<IFilterAsyncHandler, Task> action)
        {
            foreach (IFilterAsyncHandler handler in this.filters)
                await action(handler);
        }

        public async ValueTask DisposeAsync()
        {
            this.wasDisposed = true;

            if (this.connection != null)
            {
                try
                {
                    this.ApplyConnectionFilters(h => h.OnConnectionClosing);
                }
                finally
                {
                    await this.connection.DisposeAsync();
                }

                this.ApplyConnectionFilters(h => h.OnConnectionClosed);
            }
        }
    }
}