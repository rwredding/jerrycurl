using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Jerrycurl.Data;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Data.Queries;
using Jerrycurl.Collections;
using Jerrycurl.Relations.Metadata;
using System.Data.Common;
using System.Threading;
using Jerrycurl.Data.Queries.Internal;
using System.Runtime.CompilerServices;

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

            using (AdoConnection connection = new AdoConnection(this.Options))
            {
                foreach (QueryData queryData in queries.NotNull())
                {
                    AdoHelper helper = new AdoHelper(queryData);

                    if (string.IsNullOrWhiteSpace(queryData.QueryText))
                        continue;

                    foreach (IDataReader dataReader in connection.Execute(helper))
                        adapter.AddResult(dataReader);
                }
            }

            return adapter.ToList();
        }

        public async Task<IList<TItem>> ListAsync<TItem>(QueryData query, CancellationToken cancellationToken = default) => await this.ListAsync<TItem>(new[] { query }, cancellationToken);
        public async Task<IList<TItem>> ListAsync<TItem>(IEnumerable<QueryData> queries, CancellationToken cancellationToken = default)
        {
            if (queries == null)
                throw new ArgumentNullException(nameof(queries));

            if (this.Options.Schemas == null)
                throw new InvalidOperationException("No schema builder found.");

            ResultAdapter<TItem> adapter = new ResultAdapter<TItem>(this.Options.Schemas);

#if NETSTANDARD2_1
            await
#endif
            using AdoConnection connection = new AdoConnection(this.Options);

            foreach (QueryData queryData in queries.NotNull())
            {
                AdoHelper helper = new AdoHelper(queryData);

                if (string.IsNullOrWhiteSpace(queryData.QueryText))
                    continue;

#if NETSTANDARD2_0
                    await connection.ExecuteAsync(helper, async (r) =>
                    {
                        await adapter.AddResultAsync(r, cancellationToken).ConfigureAwait(false);

                    }, cancellationToken).ConfigureAwait(false);
#elif NETSTANDARD2_1
                await foreach (DbDataReader dataReader in connection.ExecuteAsync(helper, cancellationToken))
                    await adapter.AddResultAsync(dataReader, cancellationToken);
#endif
            }

            return adapter.ToList();
        }

#if NETSTANDARD2_1
        public IAsyncEnumerable<QueryReader> EnumerateAsync(QueryData query, CancellationToken cancellationToken = default) => this.EnumerateAsync(query, cancellationToken);
        public async IAsyncEnumerable<QueryReader> EnumerateAsync(IEnumerable<QueryData> queries, [EnumeratorCancellation]CancellationToken cancellationToken = default)
        {
            if (queries == null)
                throw new ArgumentNullException(nameof(queries));

            if (this.Options.Schemas == null)
                throw new InvalidOperationException("No schema builder found.");

#if NETSTANDARD2_1
            await
#endif
            using AdoConnection connection = new AdoConnection(this.Options);

            foreach (QueryData queryData in queries.NotNull())
            {
                AdoHelper helper = new AdoHelper(queryData);

                if (string.IsNullOrWhiteSpace(queryData.QueryText))
                    continue;

                await foreach (DbDataReader dataReader in connection.ExecuteAsync(helper, cancellationToken))
                    yield return new QueryReader(dataReader, this.Options.Schemas);
            }
        }

        public IAsyncEnumerable<TItem> EnumerateAsync<TItem>(QueryData query, CancellationToken cancellationToken = default) => this.EnumerateAsync<TItem>(new[] { query }, cancellationToken);
        public async IAsyncEnumerable<TItem> EnumerateAsync<TItem>(IEnumerable<QueryData> queries, [EnumeratorCancellation]CancellationToken cancellationToken = default)
        {
            await foreach (QueryReader queryReader in this.EnumerateAsync(queries, cancellationToken))
            {
                await foreach (TItem item in queryReader.ReadAsync<TItem>(cancellationToken))
                    yield return item;
            }
        }
#endif

        public IEnumerable<TItem> Enumerate<TItem>(QueryData query) => this.Enumerate<TItem>(new[] { query });
        public IEnumerable<TItem> Enumerate<TItem>(IEnumerable<QueryData> queries) => this.Enumerate(queries).SelectMany(r => r.Read<TItem>());

        public IEnumerable<QueryReader> Enumerate(QueryData query) => this.Enumerate(new[] { query });
        public IEnumerable<QueryReader> Enumerate(IEnumerable<QueryData> queries)
        {
            if (queries == null)
                throw new ArgumentNullException(nameof(queries));

            if (this.Options.Schemas == null)
                throw new InvalidOperationException("No schema builder found.");

            using AdoConnection connection = new AdoConnection(this.Options);

            foreach (QueryData queryData in queries.NotNull())
            {
                AdoHelper helper = new AdoHelper(queryData);

                if (string.IsNullOrWhiteSpace(queryData.QueryText))
                    continue;

                foreach (IDataReader reader in connection.Execute(helper))
                    yield return new QueryReader(reader, this.Options.Schemas);
            }
        }
    }
}
