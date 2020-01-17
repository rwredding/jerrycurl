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

        public IList<TItem> List<TItem>(QueryData query) => this.List<TItem>(new[] { query });
        public IList<TItem> List<TItem>(IEnumerable<QueryData> queries)
        {
            if (queries == null)
                throw new ArgumentNullException(nameof(queries));

            if (this.Options.Schemas == null)
                throw new InvalidOperationException("No schema store found.");

            ResultAdapter<TItem> adapter = new ResultAdapter<TItem>(this.Options.Schemas);

            using SyncSession connection = new SyncSession(this.Options);

            foreach (QueryData queryData in queries.NotNull())
            {
                Query builder = new Query(queryData);

                if (string.IsNullOrWhiteSpace(queryData.QueryText))
                    continue;

                foreach (IDataReader dataReader in connection.Execute(builder))
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

            await using AsyncSession connection = new AsyncSession(this.Options);

            foreach (QueryData queryData in queries.NotNull())
            {
                Query builder = new Query(queryData);

                if (string.IsNullOrWhiteSpace(queryData.QueryText))
                    continue;

                await foreach (DbDataReader dataReader in connection.ExecuteAsync(builder, cancellationToken).ConfigureAwait(false))
                    await adapter.AddResultAsync(dataReader, cancellationToken).ConfigureAwait(false);
            }

            return adapter.ToList();
        }

        public IAsyncEnumerable<QueryReader> EnumerateAsync(QueryData query, CancellationToken cancellationToken = default) => this.EnumerateAsync(query, cancellationToken);
        public async IAsyncEnumerable<QueryReader> EnumerateAsync(IEnumerable<QueryData> queries, [EnumeratorCancellation]CancellationToken cancellationToken = default)
        {
            if (queries == null)
                throw new ArgumentNullException(nameof(queries));

            if (this.Options.Schemas == null)
                throw new InvalidOperationException("No schema builder found.");

            await using AsyncSession connection = new AsyncSession(this.Options);

            foreach (QueryData queryData in queries.NotNull())
            {
                Query query = new Query(queryData);

                if (string.IsNullOrWhiteSpace(queryData.QueryText))
                    continue;

                await foreach (DbDataReader dataReader in connection.ExecuteAsync(query, cancellationToken).ConfigureAwait(false))
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

            using SyncSession connection = new SyncSession(this.Options);

            foreach (QueryData queryData in queries.NotNull())
            {
                Query query = new Query(queryData);

                if (string.IsNullOrWhiteSpace(queryData.QueryText))
                    continue;

                foreach (IDataReader reader in connection.Execute(query))
                    yield return new QueryReader(reader, this.Options.Schemas);
            }
        }
    }
}
