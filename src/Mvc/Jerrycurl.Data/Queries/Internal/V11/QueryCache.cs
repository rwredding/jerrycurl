using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Data.Queries.Internal.V11
{
    internal class QueryCache<TItem>
    {
        private static readonly ConcurrentDictionary<TableIdentity, object> factoryMap = new ConcurrentDictionary<TableIdentity, object>();
        private static QueryIndexer queryIndexer;
        private static readonly object typeLock = new object();

        public ListItemReader ListItem { get; private set; }
        public ListReader List { get; set; }
        public AggregateReader Aggregate { get; private set; }
        public InitializeReader Initialize { get; private set; }

        private static QueryIndexer GetIndexer(ISchemaStore schemas)
        {
            if (queryIndexer != null)
                return queryIndexer;

            lock (typeLock)
            {
                if (queryIndexer != null)
                    return queryIndexer;

                Type listType = typeof(IList<>).MakeGenericType(typeof(TItem));

                ISchema schema = schemas.GetSchema(listType);

                return (queryIndexer = new QueryIndexer(schema));
            }
        }

        public static QueryCache<TItem> Get(ISchemaStore schemas, TableIdentity heading)
        {
            return (QueryCache<TItem>)factoryMap.GetOrAdd(heading, _ =>
            {
                QueryIndexer indexer = GetIndexer(schemas);

                return new ResultCompiler(indexer).Compile<TItem>(heading);
            });
        }
    }
}
