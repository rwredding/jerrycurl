using Jerrycurl.Data.Queries.Internal;
using Jerrycurl.Data.Queries.Internal.Caching;
using Jerrycurl.Data.Queries.Internal.Compilation;
using Jerrycurl.Relations.Metadata;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Jerrycurl.Data.Queries
{
    public sealed class QueryReader
    {
        public ISchemaStore Schemas { get; }

        private readonly IDataReader syncReader;
        private readonly DbDataReader asyncReader;

        public QueryReader(ISchemaStore schemas, IDataReader dataReader)
        {
            this.Schemas = schemas ?? throw new ArgumentNullException(nameof(schemas));
            this.syncReader = dataReader ?? throw new ArgumentNullException(nameof(dataReader));
            this.asyncReader = dataReader as DbDataReader;
        }

        public async IAsyncEnumerable<TItem> ReadAsync<TItem>([EnumeratorCancellation]CancellationToken cancellationToken = default)
        {
            if (this.asyncReader == null)
                throw new QueryException("Async not available. To use async operations, instantiate with a DbDataReader instance.");

            EnumerateReader<TItem> reader = QueryCache<TItem>.GetEnumerateReader(this.Schemas, this.asyncReader);

            while (await this.asyncReader.ReadAsync(cancellationToken).ConfigureAwait(false))
                yield return reader(this.asyncReader);
        }

        public IEnumerable<TItem> Read<TItem>()
        {
            EnumerateReader<TItem> reader = QueryCache<TItem>.GetEnumerateReader(this.Schemas, this.syncReader);

            while (this.syncReader.Read())
                yield return reader(this.syncReader);
        }
    }
}