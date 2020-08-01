using Jerrycurl.Data.Queries.Internal.State;
using Jerrycurl.Data.Queries.Internal.V11;
using Jerrycurl.Data.Queries.Internal.V11.Factories;
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
        public ISchemaStore Schemas { get; }
        public QueryType QueryType { get; set; }

        AggregateIdentity IQueryBuffer.Aggregator => this.aggregator;
        ElasticArray IQueryBuffer.Slots => this.slots;
        ElasticArray IQueryBuffer.Aggregates => this.values;

        private readonly AggregateIdentity aggregator = new AggregateIdentity();
        private readonly ElasticArray slots = new ElasticArray();
        private readonly ElasticArray values = new ElasticArray();
        private readonly Func<IDataReader, BufferWriter> writerFactory;
        private readonly ISchema schema;

        public QueryBuffer(ISchemaStore schemas, QueryType queryType)
        {
            this.Schemas = schemas ?? throw new ArgumentNullException(nameof(schemas));
            this.QueryType = queryType;
            this.writerFactory = this.GetWriterFactory();
            this.schema = schemas.GetSchema(typeof(IList<TItem>));
        }

        private Func<IDataReader, BufferWriter> GetWriterFactory() => this.QueryType switch
        {
            QueryType.List => dr => QueryCache<TItem>.GetListWriter(this.schema, dr),
            QueryType.Aggregate => dr => QueryCache<TItem>.GetAggregateWriter(this.schema, dr),
            _ => throw new InvalidOperationException($"Invalid query type '{this.QueryType}'."),
        };

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
            AggregateIdentity identity = this.aggregator;
            AggregateReader<TItem> reader = QueryCache<TItem>.GetAggregateReader(identity);

            return reader(this);
        }

        public IList<TItem> ToList() => (IList<TItem>)this.slots[0];
    }
}
