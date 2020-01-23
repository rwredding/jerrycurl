using Jerrycurl.Collections;
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
        private readonly IFilterAsyncHandler[] filters;

        private bool wasDisposed = false;
        private bool wasOpened = false;

        public AsyncSession(SessionOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            this.connectionBase = options?.ConnectionFactory?.Invoke();
            this.connection = this.connectionBase as DbConnection;
            this.VerifyConnection();

            this.filters = options?.Filters.Select(f => f.GetAsyncHandler(this.connectionBase)).NotNull().ToArray() ?? Array.Empty<IFilterAsyncHandler>();
        }

        private void VerifyConnection()
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
            DbCommand dbCommand = null;

            Task applyException(Exception ex) => this.ApplyFilters(h => h.OnExceptionAsync, dbCommand: dbCommand, exception: ex, source: operation.Source, swallowExceptions: true);

            try
            {
                dbCommand = connection.CreateCommand();

                operation.Build(dbCommand);
            }
            catch (Exception ex)
            {
                await applyException(ex).ConfigureAwait(false);

                dbCommand?.Dispose();

                throw;
            }

            await this.ApplyFilters(h => h.OnCommandCreatedAsync, dbCommand: dbCommand, source: operation.Source).ConfigureAwait(false);

            DbDataReader reader = null;

            try
            {
                reader = dbCommand.ExecuteReader();
            }
            catch (Exception ex)
            {
                await applyException(ex).ConfigureAwait(false);

                throw;
            }

            try
            {
                bool nextResult = true;

                while (nextResult)
                {
                    yield return reader;

                    try
                    {
                        nextResult = await reader.NextResultAsync().ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        await applyException(ex).ConfigureAwait(false);

                        throw;
                    }
                }

                await this.ApplyFilters(h => h.OnCommandExecutedAsync, dbCommand: dbCommand, source: operation.Source).ConfigureAwait(false);
            }
            finally
            {
#if !NETSTANDARD2_0
                await reader.DisposeAsync().ConfigureAwait(false);
#else
                reader.Dispose();
#endif
            }
        }

        private async Task<DbConnection> GetOpenConnectionAsync(CancellationToken cancellationToken)
        {
            if (this.wasDisposed)
                throw new ObjectDisposedException("Session is no longer usable; it is disposed.");

            if (this.wasOpened)
                return this.connection;

            if (!this.wasOpened)
            {
                await this.ApplyFilters(h => h.OnConnectionOpeningAsync).ConfigureAwait(false);
                await this.connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                await this.ApplyFilters(h => h.OnConnectionOpenedAsync).ConfigureAwait(false);

                this.wasOpened = true;
            }

            return this.connection;
        }

        private async Task ApplyFilters(Func<IFilterAsyncHandler, Func<FilterContext, Task>> action, IDbCommand dbCommand = null, Exception exception = null, object source = null, bool swallowExceptions = false)
        {
            if (this.filters.Length > 0)
            {
                FilterContext context = new FilterContext(this.connection, dbCommand, exception, source);

                await this.ApplyFilters(action, context, swallowExceptions).ConfigureAwait(false);
            }
        }

        private async Task ApplyFilters(Func<IFilterAsyncHandler, Func<FilterContext, Task>> action, FilterContext context, bool swallowExceptions = false)
        {
            foreach (IFilterAsyncHandler handler in this.filters)
            {
                try
                {
                    await action(handler)(context).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    if (!swallowExceptions)
                    {
                        FilterContext exceptionContext = new FilterContext(context.Connection, context.Command, ex, context.SourceObject);

                        await this.ApplyFilters(h => h.OnExceptionAsync, exceptionContext, swallowExceptions: true).ConfigureAwait(false);

                        throw;
                    }
                }
            }
        }

        private async ValueTask DisposeFiltersAsync()
        {
            List<Exception> exceptions = new List<Exception>();

            foreach (IFilterAsyncHandler asyncHandler in this.filters)
            {
                try
                {
                    await asyncHandler.DisposeAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            if (exceptions.Count == 1)
                throw exceptions[0];
            else if (exceptions.Count > 1)
                throw new AggregateException(exceptions);
        }

        public async ValueTask DisposeAsync()
        {
            if (!this.wasDisposed)
            {
                try
                {
                    if (this.wasOpened)
                        await this.ApplyFilters(h => h.OnConnectionClosingAsync).ConfigureAwait(false);

                    try
                    {
                        this.connection?.Close();
                    }
                    catch (Exception ex)
                    {
                        await this.ApplyFilters(h => h.OnExceptionAsync, exception: ex, swallowExceptions: true).ConfigureAwait(false);

                        throw;
                    }

                    if (this.wasOpened)
                        await this.ApplyFilters(h => h.OnConnectionClosedAsync).ConfigureAwait(false);
                }
                finally
                {
                    this.wasDisposed = true;

                    try
                    {
                        await this.DisposeFiltersAsync().ConfigureAwait(false);
                    }
                    finally
                    {
#if !NETSTANDARD2_0
                    if (this.connection != null)
                        await this.connection.DisposeAsync().ConfigureAwait(false);
#else
                        this.connection?.Dispose();
#endif
                    }
                }
            }
        }
    }
}