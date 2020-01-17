using System;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Data.Queries.Internal.State
{
    internal class TypeState
    {
        public ISchema Schema { get; }
        public IndexState Indexer { get; } = new IndexState();
        public AggregateState Aggregate { get; }

        public TypeState(ISchema schema)
        {
            this.Schema = schema ?? throw new ArgumentNullException(nameof(schema));
            this.Aggregate = new AggregateState(this);
            this.Aggregate.UpdateFactory();
        }
    }
}
