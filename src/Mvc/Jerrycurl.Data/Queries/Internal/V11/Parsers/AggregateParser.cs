using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jerrycurl.Collections;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Data.Queries.Internal.V11.Parsers
{
    internal class AggregateParser
    {
        public ISchema Schema { get; }
        public QueryIndexer Indexer { get; }

        public AggregateParser(ISchema schema, QueryIndexer indexer)
        {
            this.Schema = schema ?? throw new ArgumentNullException(nameof(schema));
            this.Indexer = indexer ?? throw new ArgumentException(nameof(indexer));
        }

        public AggregateTree Parse(AggregateIdentity identity)
        {
            return null;
        }
    }
}
