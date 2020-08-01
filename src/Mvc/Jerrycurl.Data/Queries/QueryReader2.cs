using Jerrycurl.Data.Queries.Internal;
using Jerrycurl.Data.Queries.Internal.State;
using Jerrycurl.Data.Queries.Internal.V11;
using Jerrycurl.Data.Queries.Internal.V11.Factories;
using Jerrycurl.Relations.Metadata;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Jerrycurl.Data.Queries
{
    public class QueryReader2
    {
        public ISchemaStore Schemas { get; }

        private readonly IDataReader syncReader;
        private readonly DbDataReader asyncReader;

        public QueryReader2(ISchemaStore schemas, IDataReader dataReader)
        {
            this.Schemas = schemas ?? throw new ArgumentNullException(nameof(schemas));
            this.syncReader = dataReader ?? throw new ArgumentNullException(nameof(dataReader));
            this.asyncReader = dataReader as DbDataReader;
        }

        public async IAsyncEnumerable<TItem> ReadAsync<TItem>([EnumeratorCancellation]CancellationToken cancellationToken = default)
        {
            if (this.asyncReader == null)
                throw new QueryException("Async not available. To use async operations, please supply a DbDataReader instance.");

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