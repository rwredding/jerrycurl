using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Jerrycurl.Collections;
using System.Data.Common;
using System.Threading;
using Jerrycurl.Data.Queries.Internal;
using System.Runtime.CompilerServices;
using Jerrycurl.Data.Sessions;

namespace Jerrycurl.Data.Queries
{
    public class QueryHandler
    {
        public QueryOptions Options { get; }

        public QueryHandler(QueryOptions options)
        {
            this.Options = options ?? throw new ArgumentNullException(nameof(options));
        }

        #region " Aggregate "

        public T Aggregate<T>(QueryData query) => this.Aggregate<T>(new[] { query });
        public T Aggregate<T>(IEnumerable<QueryData> queries)
        {
            if (queries == null)
                throw new ArgumentNullException(nameof(queries));

            if (this.Options.Schemas == null)
                throw new InvalidOperationException("No schema store found.");

            ResultAdapter<T> adapter = new ResultAdapter<T>(this.Options.Schemas);

            using ISyncSession connection = this.Options.GetSyncSession();

            foreach (Query operation in this.GetOperations(queries))
            {
                foreach (IDataReader dataReader in connection.Execute(operation))
                    adapter.AddResult(dataReader);
            }

            return adapter.ToAggregate();
        }

        public Task<T> AggregateAsync<T>(QueryData query, CancellationToken cancellationToken = default) => this.AggregateAsync<T>(new[] { query }, cancellationToken);

        public async Task<T> AggregateAsync<T>(IEnumerable<QueryData> queries, CancellationToken cancellationToken = default)
        {
            if (queries == null)
                throw new ArgumentNullException(nameof(queries));

            if (this.Options.Schemas == null)
                throw new InvalidOperationException("No schema builder found.");

            ResultAdapter<T> adapter = new ResultAdapter<T>(this.Options.Schemas);

            await using IAsyncSession connection = this.Options.GetAsyncSession();

            foreach (Query operation in this.GetOperations(queries))
            {
                await foreach (DbDataReader dataReader in connection.ExecuteAsync(operation, cancellationToken).ConfigureAwait(false))
                    await adapter.AddResultAsync(dataReader, cancellationToken).ConfigureAwait(false);
            }

            return adapter.ToAggregate();
        }

        #endregion

        #region " List "

        public IList<TItem> List<TItem>(QueryData query) => this.List<TItem>(new[] { query });
        public IList<TItem> List<TItem>(IEnumerable<QueryData> queries)
        {
            if (queries == null)
                throw new ArgumentNullException(nameof(queries));

            if (this.Options.Schemas == null)
                throw new InvalidOperationException("No schema store found.");

            ResultAdapter<TItem> adapter = new ResultAdapter<TItem>(this.Options.Schemas);

            using ISyncSession connection = this.Options.GetSyncSession();

            foreach (Query operation in this.GetOperations(queries))
            { 
                foreach (IDataReader dataReader in connection.Execute(operation))
                    adapter.AddResult(dataReader);
            }

            return adapter.ToList();
        }

        public Task<IList<TItem>> ListAsync<TItem>(QueryData query, CancellationToken cancellationToken = default) => this.ListAsync<TItem>(new[] { query }, cancellationToken);

        public async Task<IList<TItem>> ListAsync<TItem>(IEnumerable<QueryData> queries, CancellationToken cancellationToken = default)
        {
            if (queries == null)
                throw new ArgumentNullException(nameof(queries));

            if (this.Options.Schemas == null)
                throw new InvalidOperationException("No schema builder found.");

            ResultAdapter<TItem> adapter = new ResultAdapter<TItem>(this.Options.Schemas);

            await using IAsyncSession connection = this.Options.GetAsyncSession();

            foreach (Query operation in this.GetOperations(queries))
            {
                await foreach (DbDataReader dataReader in connection.ExecuteAsync(operation, cancellationToken).ConfigureAwait(false))
                    await adapter.AddResultAsync(dataReader, cancellationToken).ConfigureAwait(false);
            }

            return adapter.ToList();
        }

        #endregion

        #region " Enumerate "

        public IAsyncEnumerable<QueryReader> EnumerateAsync(QueryData query, CancellationToken cancellationToken = default) => this.EnumerateAsync(query, cancellationToken);
        public async IAsyncEnumerable<QueryReader> EnumerateAsync(IEnumerable<QueryData> queries, [EnumeratorCancellation]CancellationToken cancellationToken = default)
        {
            if (queries == null)
                throw new ArgumentNullException(nameof(queries));

            if (this.Options.Schemas == null)
                throw new InvalidOperationException("No schema builder found.");

            await using IAsyncSession connection = this.Options.GetAsyncSession();

            foreach (Query operation in this.GetOperations(queries))
            {
                await foreach (DbDataReader dataReader in connection.ExecuteAsync(operation, cancellationToken).ConfigureAwait(false))
                    yield return new QueryReader(dataReader, this.Options.Schemas);
            }
        }

        public IAsyncEnumerable<TItem> EnumerateAsync<TItem>(QueryData query, CancellationToken cancellationToken = default) => this.EnumerateAsync<TItem>(new[] { query }, cancellationToken);
        public async IAsyncEnumerable<TItem> EnumerateAsync<TItem>(IEnumerable<QueryData> queries, [EnumeratorCancellation]CancellationToken cancellationToken = default)
        {
            await foreach (QueryReader queryReader in this.EnumerateAsync(queries, cancellationToken).ConfigureAwait(false))
            {
                await foreach (TItem item in queryReader.ReadAsync<TItem>(cancellationToken).ConfigureAwait(false))
                    yield return item;
            }
        }

        public IEnumerable<TItem> Enumerate<TItem>(QueryData query) => this.Enumerate<TItem>(new[] { query });
        public IEnumerable<TItem> Enumerate<TItem>(IEnumerable<QueryData> queries) => this.Enumerate(queries).SelectMany(r => r.Read<TItem>());

        public IEnumerable<QueryReader> Enumerate(QueryData query) => this.Enumerate(new[] { query });
        public IEnumerable<QueryReader> Enumerate(IEnumerable<QueryData> queries)
        {
            if (queries == null)
                throw new ArgumentNullException(nameof(queries));

            if (this.Options.Schemas == null)
                throw new InvalidOperationException("No schema builder found.");

            using ISyncSession connection = this.Options.GetSyncSession();

            foreach (Query operation in this.GetOperations(queries))
            {
                foreach (IDataReader reader in connection.Execute(operation))
                    yield return new QueryReader(reader, this.Options.Schemas);
            }
        }

        #endregion

        private IEnumerable<Query> GetOperations(IEnumerable<QueryData> queries)
            => queries.NotNull().Where(d => !string.IsNullOrWhiteSpace(d.QueryText)).Select(d => new Query(d));
    }
}
