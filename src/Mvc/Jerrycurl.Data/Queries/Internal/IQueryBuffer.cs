using Jerrycurl.Data.Sessions;

namespace Jerrycurl.Data.Queries.Internal
{
    internal interface IQueryBuffer
    {
        public AggregateBuffer Aggregate { get; }
        public ElasticArray Slots { get; }
    }
}
