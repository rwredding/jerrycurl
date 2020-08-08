using Jerrycurl.Data.Queries.Internal;
using Jerrycurl.Data.Queries.Internal.Caching;
using Jerrycurl.Data.Queries.Internal.Compilation;
using Jerrycurl.Data.Sessions;
using Jerrycurl.Relations.Metadata;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Jerrycurl.Data.Queries
{
    public sealed class QueryBuffer<TItem> : IQueryBuffer
    {
        public ISchema Schema { get; }
        public QueryType QueryType { get; set; }

        AggregateBuffer IQueryBuffer.Aggregate => this.aggregate;
        ElasticArray IQueryBuffer.Slots => this.slots;

        private readonly AggregateBuffer aggregate;
        private readonly ElasticArray slots = new ElasticArray();
        private readonly Func<IDataReader, BufferWriter> writerFactory;

        public QueryBuffer(ISchemaStore schemas, QueryType queryType)
        {
            this.Schema = schemas?.GetSchema(typeof(IList<TItem>)) ?? throw new ArgumentNullException(nameof(schemas));
            this.QueryType = queryType;
            this.aggregate = new AggregateBuffer(this.Schema);
            this.writerFactory = this.GetWriterFactory();
        }

        private Func<IDataReader, BufferWriter> GetWriterFactory() => this.QueryType switch
        {
            QueryType.List => dataReader => QueryCache<TItem>.GetListWriter(this.Schema, dataReader),
            QueryType.Aggregate => dataReader => QueryCache<TItem>.GetAggregateWriter(this.Schema, dataReader),
            _ => throw new InvalidOperationException($"Invalid query type '{this.QueryType}'."),
        };

        public IOperation Read(QueryData queryData) => new Query(queryData);

        public void Write(IDataReader dataReader)
        {
            BufferWriter writer = this.writerFactory(dataReader);

            writer.WriteAll(this, dataReader);
        }

        public async Task WriteAsync(DbDataReader dataReader, CancellationToken cancellationToken)
        {
            BufferWriter writer = this.writerFactory(dataReader);

            writer.Initialize(this);

            while (await dataReader.ReadAsync(cancellationToken).ConfigureAwait(false))
                writer.WriteOne(this, dataReader);
        }

        public TItem ToAggregate()
        {
            QueryCacheKey<AggregateName> cacheKey = this.aggregate.ToCacheKey();
            AggregateReader<TItem> reader = QueryCache<TItem>.GetAggregateReader(cacheKey);

            return reader(this);
        }

        public IList<TItem> ToList() => (IList<TItem>)this.slots[0] ?? new List<TItem>();
    }
}
