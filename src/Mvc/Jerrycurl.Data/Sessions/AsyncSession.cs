using Jerrycurl.Data.Filters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Jerrycurl.Data.Sessions
{
    public class AsyncSession : IAsyncSession
    {
        private readonly IDbConnection connectionBase;
        private readonly DbConnection connection;
        private readonly (IFilterHandler, IFilterAsyncHandler)[] filters;

        private bool wasDisposed = false;
        private bool wasOpened = false;

        public AsyncSession(SessionOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            this.connectionBase = options?.ConnectionFactory?.Invoke();
            this.connection = this.connectionBase as DbConnection;
            this.filters = options?.Filters.Select(f => (f.GetHandler(this.connectionBase), f.GetAsyncHandler(this.connectionBase))).ToArray() ?? Array.Empty<(IFilterHandler, IFilterAsyncHandler)>();

            this.VerifyAsyncSetup();
        }

        private void VerifyAsyncSetup()
        {
            if (this.connectionBase == null)
                throw new InvalidOperationException("No connection returned from ConnectionFactory.");

            if (this.connection == null)
                throw new NotSupportedException("Async operations are only available for DbConnection instances.");

            if (this.connection.State != ConnectionState.Closed)
                throw new InvalidOperationException("Connection is managed automatically and should be initially closed.");
        }

        public async IAsyncEnumerable<DbDataReader> ExecuteAsync(IOperation operation, [EnumeratorCancellation]CancellationToken cancellationToken)
        {
            DbConnection connection = await this.GetOpenConnectionAsync(cancellationToken).ConfigureAwait(false);

            using DbCommand dbCommand = connection.CreateCommand();

            try
            {
                operation.Build(dbCommand);
            }
            catch (Exception ex)
            {
                await this.ApplyCommandFiltersAsync(h => h.OnException, h => h.OnExceptionAsync, dbCommand, ex, operation.Source);

                throw;
            }

            if (this.filters.Length > 0)
                await this.ApplyCommandFiltersAsync(h => h.OnCommandCreated, h => h.OnCommandCreatedAsync, dbCommand, source: operation.Source);

            DbDataReader reader = null;

            try
            {
                reader = await dbCommand.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await this.ApplyCommandFiltersAsync(h => h.OnException, h => h.OnExceptionAsync, dbCommand, ex, operation.Source);

                throw;
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

            if (this.filters.Length > 0)
                await this.ApplyCommandFiltersAsync(h => h.OnCommandExecuted, h => h.OnCommandExecutedAsync, dbCommand, source: operation.Source);
        }

        private async Task<DbConnection> GetOpenConnectionAsync(CancellationToken cancellationToken)
        {
            if (this.wasDisposed)
                throw new ObjectDisposedException("Connection is no longer usable; it is disposed.");

            if (this.wasOpened)
                return this.connection;

            try
            {
                if (!this.wasOpened)
                {
                    await this.ApplyConnectionFiltersAsync(h => h.OnConnectionOpening, h => h.OnConnectionOpeningAsync);
                    await this.connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                    await this.ApplyConnectionFiltersAsync(h => h.OnConnectionOpened, h => h.OnConnectionOpenedAsync);
                }
            }
            finally
            {
                this.wasOpened = true;
            }

            return this.connection;
        }

        private async Task ApplyCommandFiltersAsync(Func<IFilterHandler, Action<FilterContext>> action, Func<IFilterAsyncHandler, Func<FilterContext, Task>> asyncAction, IDbCommand command, Exception exception = null, object source = null)
        {
            FilterContext context = new FilterContext(command, exception, source);

            await this.ApplyFiltersAsync(h => action(h)(context), h => asyncAction(h)(context));
        }
            

        private async Task ApplyConnectionFiltersAsync(Func<IFilterHandler, Action<FilterContext>> action, Func<IFilterAsyncHandler, Func<FilterContext, Task>> asyncAction, Exception exception = null)
        {
            FilterContext context = new FilterContext(this.connection, exception, null);

            await this.ApplyFiltersAsync(h => action(h)(context), h => asyncAction(h)(context));
        }

        private async Task ApplyFiltersAsync(Action<IFilterHandler> action, Func<IFilterAsyncHandler, Task> asyncAction)
        {
            foreach (var (handler, asyncHandler) in this.filters)
            {
                if (asyncHandler != null)
                    await asyncAction(asyncHandler);
                else if (handler != null)
                    action(handler);
            }
        }

        private void ApplyFilters(Func<IFilterHandler, Action<FilterContext>> action, FilterContext context)
        {
            foreach (var (handler, _) in this.filters)
            {
                if (handler != null)
                    action(handler)(context);
            }
        }

        private void DisposeFilters()
        {
            foreach (var (handler, asyncHandler) in this.filters)
            {
                if (asyncHandler != null)
                    asyncHandler.Dispose();
                else if (handler != null)
                    handler.Dispose();
            }
        }

        private async Task DisposeFiltersAsync()
        {
            foreach (var (handler, asyncHandler) in this.filters)
            {
                if (asyncHandler != null)
                    await asyncHandler.DisposeAsync();
                else if (handler != null)
                    handler.Dispose();
            }
        }

        

        public void Dispose()
        {
            if (!this.wasDisposed)
            {
                try
                {
                    if (this.wasOpened)
                        this.ApplyFilters(h => h.OnConnectionClosing, new FilterContext(this.connection, null));
                }
                finally
                {
                    this.connection?.Dispose();
                }

                try
                {
                    if (this.wasOpened)
                        this.ApplyFilters(h => h.OnConnectionClosed, new FilterContext(this.connection, null));
                }
                finally
                {
                    this.DisposeFilters();

                    this.wasDisposed = true;
                }

            }
        }

        public async ValueTask DisposeAsync()
        {
            if (!this.wasDisposed)
            {
                try
                {
                    if (this.wasOpened)
                        await this.ApplyConnectionFiltersAsync(h => h.OnConnectionClosing, h => h.OnConnectionClosingAsync);
                }
                finally
                {
#if !NETSTANDARD2_0
                    if (this.connection != null)
                        await this.connection.DisposeAsync();
#else
                    this.connection?.Dispose();
#endif
                }

                try
                {
                    if (this.wasOpened)
                        await this.ApplyConnectionFiltersAsync(h => h.OnConnectionClosed, h => h.OnConnectionClosedAsync);
                }
                finally
                {
                    await this.DisposeFiltersAsync();

                    this.wasDisposed = true;
                }
            }
        }
    }
}