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
using Jerrycurl.Data.Queries.Internal.V11;

namespace Jerrycurl.Data.Queries
{
    public class QueryEngine
    {
        public QueryOptions Options { get; }

        public QueryEngine(QueryOptions options)
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

            QueryBuffer<T> result = new QueryBuffer<T>(this.Options.Schemas, QueryType.Aggregate);

            using ISyncSession connection = this.Options.GetSyncSession();

            foreach (Query operation in this.GetOperations(queries))
            {
                foreach (IDataReader dataReader in connection.Execute(operation))
                    result.Write(dataReader);
            }

            return result.ToAggregate();
        }

        public Task<T> AggregateAsync<T>(QueryData query, CancellationToken cancellationToken = default) => this.AggregateAsync<T>(new[] { query }, cancellationToken);

        public async Task<T> AggregateAsync<T>(IEnumerable<QueryData> queries, CancellationToken cancellationToken = default)
        {
            if (queries == null)
                throw new ArgumentNullException(nameof(queries));

            if (this.Options.Schemas == null)
                throw new InvalidOperationException("No schema builder found.");

            QueryBuffer<T> result = new QueryBuffer<T>(this.Options.Schemas, QueryType.Aggregate);

            await using IAsyncSession connection = this.Options.GetAsyncSession();

            foreach (Query operation in this.GetOperations(queries))
            {
                await foreach (DbDataReader dataReader in connection.ExecuteAsync(operation, cancellationToken).ConfigureAwait(false))
                    await result.WriteAsync(dataReader, cancellationToken).ConfigureAwait(false);
            }

            return result.ToAggregate();
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

            QueryBuffer<TItem> buffer = new QueryBuffer<TItem>(this.Options.Schemas, QueryType.List);

            using ISyncSession connection = this.Options.GetSyncSession();

            foreach (Query operation in this.GetOperations(queries))
            { 
                foreach (IDataReader dataReader in connection.Execute(operation))
                    buffer.Write(dataReader);
            }

            return buffer.ToList();
        }

        public Task<IList<TItem>> ListAsync<TItem>(QueryData query, CancellationToken cancellationToken = default) => this.ListAsync<TItem>(new[] { query }, cancellationToken);

        public async Task<IList<TItem>> ListAsync<TItem>(IEnumerable<QueryData> queries, CancellationToken cancellationToken = default)
        {
            if (queries == null)
                throw new ArgumentNullException(nameof(queries));

            if (this.Options.Schemas == null)
                throw new InvalidOperationException("No schema builder found.");

            QueryBuffer<TItem> result = new QueryBuffer<TItem>(this.Options.Schemas, QueryType.List);

            await using IAsyncSession connection = this.Options.GetAsyncSession();

            foreach (Query operation in this.GetOperations(queries))
            {
                await foreach (DbDataReader dataReader in connection.ExecuteAsync(operation, cancellationToken).ConfigureAwait(false))
                    await result.WriteAsync(dataReader, cancellationToken).ConfigureAwait(false);
            }

            return result.ToList();
        }

        #endregion

        #region " Enumerate "

        public IAsyncEnumerable<QueryReader2> EnumerateAsync(QueryData query, CancellationToken cancellationToken = default) => this.EnumerateAsync(query, cancellationToken);
        public async IAsyncEnumerable<QueryReader2> EnumerateAsync(IEnumerable<QueryData> queries, [EnumeratorCancellation]CancellationToken cancellationToken = default)
        {
            if (queries == null)
                throw new ArgumentNullException(nameof(queries));

            if (this.Options.Schemas == null)
                throw new InvalidOperationException("No schema builder found.");

            await using IAsyncSession connection = this.Options.GetAsyncSession();

            foreach (Query operation in this.GetOperations(queries))
            {
                await foreach (DbDataReader dataReader in connection.ExecuteAsync(operation, cancellationToken).ConfigureAwait(false))
                    yield return new QueryReader2(this.Options.Schemas, dataReader);
            }
        }

        public IAsyncEnumerable<TItem> EnumerateAsync<TItem>(QueryData query, CancellationToken cancellationToken = default) => this.EnumerateAsync<TItem>(new[] { query }, cancellationToken);
        public async IAsyncEnumerable<TItem> EnumerateAsync<TItem>(IEnumerable<QueryData> queries, [EnumeratorCancellation]CancellationToken cancellationToken = default)
        {
            await foreach (QueryReader2 queryReader in this.EnumerateAsync(queries, cancellationToken).ConfigureAwait(false))
            {
                await foreach (TItem item in queryReader.ReadAsync<TItem>(cancellationToken).ConfigureAwait(false))
                    yield return item;
            }
        }

        public IEnumerable<TItem> Enumerate<TItem>(QueryData query) => this.Enumerate<TItem>(new[] { query });
        public IEnumerable<TItem> Enumerate<TItem>(IEnumerable<QueryData> queries) => this.Enumerate(queries).SelectMany(r => r.Read<TItem>());

        public IEnumerable<QueryReader2> Enumerate(QueryData query) => this.Enumerate(new[] { query });
        public IEnumerable<QueryReader2> Enumerate(IEnumerable<QueryData> queries)
        {
            if (queries == null)
                throw new ArgumentNullException(nameof(queries));

            if (this.Options.Schemas == null)
                throw new InvalidOperationException("No schema builder found.");

            using ISyncSession connection = this.Options.GetSyncSession();

            foreach (Query operation in this.GetOperations(queries))
            {
                foreach (IDataReader reader in connection.Execute(operation))
                    yield return new QueryReader2(this.Options.Schemas, reader);
            }
        }

        #endregion

        private IEnumerable<Query> GetOperations(IEnumerable<QueryData> queries)
            => queries.NotNull().Where(d => !string.IsNullOrWhiteSpace(d.QueryText)).Select(d => new Query(d));
    }
}
