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

        public SyncSession(SessionOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            this.connection = options?.ConnectionFactory?.Invoke();
            this.filters = options?.Filters.Select(f => f.GetHandler(this.connection)).ToArray() ?? Array.Empty<IFilterHandler>();

            this.VerifySetup();
        }

        private void VerifySetup()
        {
            if (this.connection == null)
                throw new InvalidOperationException("No connection returned from ConnectionFactory.");

            if (this.connection.State != ConnectionState.Closed)
                throw new InvalidOperationException("Connection is managed automatically and should be initially closed.");
        }

        public IEnumerable<IDataReader> Execute(IOperation operation)
        {
            IDbConnection connection = this.GetOpenConnection();

            using IDbCommand dbCommand = connection.CreateCommand();

            try
            {
                operation.Build(dbCommand);
            }
            catch (Exception ex)
            {
                this.ApplyCommandFilters(h => h.OnException, dbCommand, ex, operation.Source);

                throw;
            }

            if (this.filters.Length > 0)
                this.ApplyCommandFilters(h => h.OnCommandCreated, dbCommand, source: operation.Source);

            IDataReader reader = null;

            try
            {
                reader = dbCommand.ExecuteReader();
            }
            catch (Exception ex)
            {
                this.ApplyCommandFilters(h => h.OnException, dbCommand, ex, operation.Source);

                throw;
            }

            try
            {
                do yield return reader; 
                while (reader.NextResult());
            }
            finally
            {
                reader?.Dispose();
            }

            if (this.filters.Length > 0)
                this.ApplyCommandFilters(h => h.OnCommandExecuted, dbCommand, source: operation.Source);
        }

        private IDbConnection GetOpenConnection()
        {
            if (this.wasDisposed)
                throw new ObjectDisposedException("Connection is no longer usable; it is disposed.");

            if (this.wasOpened)
                return this.connection;

            try
            {
                if (!this.wasOpened)
                {
                    this.ApplyConnectionFilters(h => h.OnConnectionOpening);
                    this.connection.Open();
                    this.ApplyConnectionFilters(h => h.OnConnectionOpened);
                }
            }
            finally
            {
                this.wasOpened = true;
            }

            return this.connection;
        }


        private void ApplyCommandFilters(Func<IFilterHandler, Action<FilterContext>> action, IDbCommand command, Exception exception = null, object source = null)
        {
            FilterContext context = new FilterContext(command, exception, source);

            this.ApplyFilters(action, context);
        }

        private void ApplyConnectionFilters(Func<IFilterHandler, Action<FilterContext>> action, Exception exception = null)
        {
            FilterContext context = new FilterContext(this.connection, exception, null);

            this.ApplyFilters(action, context);
        }

        private void ApplyFilters(Func<IFilterHandler, Action<FilterContext>> action, FilterContext context)
        {
            foreach (IFilterHandler handler in this.filters.NotNull())
                action(handler)(context);
        }

        private void DisposeFilters()
        {
            foreach (IFilterHandler handler in this.filters.NotNull())
                handler.Dispose();
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
    }
}
