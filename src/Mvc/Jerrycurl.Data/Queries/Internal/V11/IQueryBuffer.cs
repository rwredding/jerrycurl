namespace Jerrycurl.Data.Queries.Internal.V11
{
    internal interface IQueryBuffer
    {
        public AggregateBuffer Aggregator { get; }

        public ElasticArray Slots { get; }
        public ElasticArray Aggregates { get; }
    }
}
