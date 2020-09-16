using Jerrycurl.Collections;
using Jerrycurl.Data.Filters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Jerrycurl.Data.Sessions
{
    public partial class SyncSession : ISyncSession
    {
        private readonly IDbConnection connection;
        private readonly IFilterHandler[] filters;

        private bool wasDisposed = false;
        private bool wasOpened = false;

        public SyncSession(Func<IDbConnection> connectionFactory, ICollection<IFilter> filters)
        {
            if (connectionFactory == null)
                throw new ArgumentNullException(nameof(connectionFactory));

            this.connection = connectionFactory.Invoke();
            this.VerifyConnection();

            this.filters = filters?.Select(f => f.GetHandler(this.connection)).NotNull().ToArray() ?? Array.Empty<IFilterHandler>();
        }

        private void VerifyConnection()
        {
            if (this.connection == null)
                throw new InvalidOperationException("No connection returned from ConnectionFactory.");

            if (this.connection.State != ConnectionState.Closed)
                throw new InvalidOperationException("Connection is managed automatically and should be initially closed.");
        }

        public IEnumerable<IDataReader> Execute(IBatch batch)
        {
            IDbConnection connection = this.GetOpenConnection();
            IDbCommand dbCommand = null;

            void applyException(Exception ex) => this.ApplyFilters(h => h.OnException, dbCommand: dbCommand, exception: ex, batch: batch, swallowExceptions: true);

            try
            {
                dbCommand = connection.CreateCommand();

                batch.Build(dbCommand);
            }
            catch (Exception ex)
            {
                applyException(ex);

                dbCommand?.Dispose();

                throw;
            }

            this.ApplyFilters(h => h.OnCommandCreated, dbCommand: dbCommand, batch: batch);

            IDataReader reader = null;

            try
            {
                reader = dbCommand.ExecuteReader();
            }
            catch (Exception ex)
            {
                applyException(ex);

                throw;
            }

            return innerReader();

            IEnumerable<IDataReader> innerReader()
            {
                try
                {
                    bool nextResult = true;

                    while (nextResult)
                    {
                        yield return reader;

                        try
                        {
                            nextResult = reader.NextResult();
                        }
                        catch (Exception ex)
                        {
                            applyException(ex);

                            throw;
                        }
                    }

                    this.ApplyFilters(h => h.OnCommandExecuted, dbCommand, batch: batch);
                }
                finally
                {
                    reader.Dispose();
                }
            }
        }

        private IDbConnection GetOpenConnection()
        {
            if (this.wasDisposed)
                throw new ObjectDisposedException("Session is no longer usable; it is disposed.");

            if (this.wasOpened)
                return this.connection;

            if (!this.wasOpened)
            {
                this.ApplyFilters(h => h.OnConnectionOpening);
                this.connection.Open();
                this.ApplyFilters(h => h.OnConnectionOpened);

                this.wasOpened = true;
            }

            return this.connection;
        }

        private void ApplyFilters(Func<IFilterHandler, Action<FilterContext>> action, IDbCommand dbCommand = null, Exception exception = null, IBatch batch = null, bool swallowExceptions = false)
        {
            if (this.filters.Length > 0)
            {
                FilterContext context = new FilterContext(this.connection, dbCommand, exception, batch);

                this.ApplyFilters(action, context, swallowExceptions);
            }
        }

        private void ApplyFilters(Func<IFilterHandler, Action<FilterContext>> action, FilterContext context, bool swallowExceptions = false)
        {
            foreach (IFilterHandler handler in this.filters)
            {
                try
                {
                    action(handler)(context);
                }
                catch (Exception ex)
                {
                    if (!swallowExceptions)
                    {
                        FilterContext exceptionContext = new FilterContext(context.Connection, context.Command, ex, context.Batch);

                        this.ApplyFilters(h => h.OnException, exceptionContext, swallowExceptions: true);

                        if (!exceptionContext.IsHandled)
                            throw;
                    }
                }
            }
        }

        private void DisposeFilters()
        {
            List<Exception> exceptions = new List<Exception>();

            foreach (IFilterHandler handler in this.filters)
            {
                try
                {
                    handler.Dispose();
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

        public void Dispose()
        {
            if (!this.wasDisposed)
            {
                try
                {
                    if (this.wasOpened)
                        this.ApplyFilters(h => h.OnConnectionClosing);

                    try
                    {
                        this.connection?.Close();
                    }
                    catch (Exception ex)
                    {
                        this.ApplyFilters(h => h.OnException, exception: ex, swallowExceptions: true);

                        throw;
                    }

                    if (this.wasOpened)
                        this.ApplyFilters(h => h.OnConnectionClosed);
                }
                finally
                {
                    this.wasDisposed = true;

                    try
                    {
                        this.DisposeFilters();
                    }
                    finally
                    {
                        this.connection?.Dispose();
                    }
                }
            }
        }
    }
}
