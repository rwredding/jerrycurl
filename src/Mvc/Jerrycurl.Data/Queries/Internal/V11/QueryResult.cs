using Jerrycurl.Data.Queries.Internal.State;
using Jerrycurl.Relations.Metadata;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Jerrycurl.Data.Queries.Internal.V11
{
    internal class QueryResult<TItem>
    {
        public ISchemaStore Schemas { get; }
        public QueryType Type { get; }
        public QueryCache<TItem> Cache { get; set; }

        public ElasticArray Lists { get; } = new ElasticArray();
        public ElasticArray Aggregates { get; } = new ElasticArray();

        public QueryResult(ISchemaStore schemas, QueryType queryType)
        {
            this.Schemas = schemas ?? throw new ArgumentNullException(nameof(schemas));
            this.Type = queryType;
        }

        private void UpdateCache(IDataReader dataReader)
        {
            TableIdentity heading = TableIdentity.FromRecord(dataReader);

            this.Cache = QueryCache<TItem>.Get(this.Schemas, heading);
        }

        public void Aggregate(IDataReader dataReader)
        {
            this.Cache.Aggregate(dataReader, this.Aggregates, this.Lists);
        }

        public void Add(IDataReader dataReader)
        {
            this.UpdateCache(dataReader);

            this.Cache.List(dataReader, this.Lists);
        }

        public async Task AddAsync(DbDataReader dataReader, CancellationToken cancellationToken)
        {
            this.UpdateCache(dataReader);

            this.Cache.Initialize(this.Lists);

            while (await dataReader.ReadAsync(cancellationToken).ConfigureAwait(false))
                this.Cache.ListItem(dataReader, this.Lists);
        }

        public TItem ToAggregate() => default;
        public IList<TItem> ToList() => (IList<TItem>)this.Lists[0];
    }
}
