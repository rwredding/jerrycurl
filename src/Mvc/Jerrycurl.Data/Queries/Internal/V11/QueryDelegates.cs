using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Jerrycurl.Data.Queries.Internal.V11
{
    internal delegate void ListItemReader(IDataReader dataReader, ElasticArray lists);
    internal delegate void ListReader(IDataReader dataReader, ElasticArray lists);
    internal delegate void AggregateReader(IDataReader dataReader, ElasticArray lists, ElasticArray aggregates);
    internal delegate void InitializeReader(ElasticArray lists);

    internal delegate TItem EnumerateItemReader<TItem>(IDataReader dataReader);
    internal delegate TItem AggregateReader<TItem>(ElasticArray lists, ElasticArray aggregates);
}
