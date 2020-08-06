using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Data.Queries.Internal.Compilation;
using Jerrycurl.Data.Queries.Internal.Parsing;
using Jerrycurl.Relations.Metadata;
using AggregateCacheKey = Jerrycurl.Data.Queries.Internal.Caching.QueryCacheKey<Jerrycurl.Data.Queries.Internal.Caching.AggregateValue>;
using ColumnCacheKey = Jerrycurl.Data.Queries.Internal.Caching.QueryCacheKey<Jerrycurl.Data.Queries.Internal.Caching.ColumnValue>;

namespace Jerrycurl.Data.Queries.Internal.Caching
{
    internal class QueryCache<TItem>
    {
        private static readonly ConcurrentDictionary<ColumnCacheKey, EnumerateReader<TItem>> enumerateReaders = new ConcurrentDictionary<ColumnCacheKey, EnumerateReader<TItem>>();
        private static readonly ConcurrentDictionary<ColumnCacheKey, BufferWriter> listWriters = new ConcurrentDictionary<ColumnCacheKey, BufferWriter>();
        private static readonly ConcurrentDictionary<ColumnCacheKey, BufferWriter> aggregrateWriters = new ConcurrentDictionary<ColumnCacheKey, BufferWriter>();
        private static readonly ConcurrentDictionary<AggregateCacheKey, AggregateReader<TItem>> aggregateReaders = new ConcurrentDictionary<AggregateCacheKey, AggregateReader<TItem>>();
        private static readonly ConcurrentDictionary<ISchema, QueryIndexer> indexers = new ConcurrentDictionary<ISchema, QueryIndexer>();

        private static QueryIndexer GetIndexer(ISchema schema) => indexers.GetOrAdd(schema, new QueryIndexer());

        public static AggregateReader<TItem> GetAggregateReader(AggregateCacheKey cacheKey)
        {
            return aggregateReaders.GetOrAdd(cacheKey, _ =>
            {
                QueryIndexer indexer = GetIndexer(cacheKey.Schema);
                AggregateParser parser = new AggregateParser(cacheKey.Schema, indexer);
                AggregateTree tree = parser.Parse(cacheKey.Items);

                QueryCompiler compiler = new QueryCompiler();

                return compiler.Compile<TItem>(tree);
            });
        }

        public static EnumerateReader<TItem> GetEnumerateReader(ISchemaStore schemas, IDataRecord dataRecord)
        {
            ISchema schema = schemas.GetSchema(typeof(IList<TItem>));
            ColumnCacheKey cacheKey = GetCacheKey(schema, dataRecord);

            return enumerateReaders.GetOrAdd(cacheKey, k =>
            {
                EnumerateParser parser = new EnumerateParser(schema);
                EnumerateTree tree = parser.Parse(k.Items);

                QueryCompiler compiler = new QueryCompiler();

                return compiler.Compile<TItem>(tree);
            });
        }

        public static BufferWriter GetAggregateWriter(ISchema schema, IDataRecord dataRecord)
        {
            ColumnCacheKey cacheKey = GetCacheKey(schema, dataRecord);

            return aggregrateWriters.GetOrAdd(cacheKey, k => GetWriter(k, QueryType.Aggregate));
        }

        public static BufferWriter GetListWriter(ISchema schema, IDataRecord dataRecord)
        {
            ColumnCacheKey cacheKey = GetCacheKey(schema, dataRecord);

            return listWriters.GetOrAdd(cacheKey, k => GetWriter(k, QueryType.List));
        }

        private static ColumnCacheKey GetCacheKey(ISchema schema, IDataRecord dataRecord)
        {
            IEnumerable<ColumnValue> columns = Enumerable.Range(0, getFieldCount()).Select(i => GetColumnValue(dataRecord, i));

            return new ColumnCacheKey(schema, columns);

            int getFieldCount()
            {
                try { return dataRecord.FieldCount; }
                catch { return 0; }
            }
        }

        public static ColumnValue GetColumnValue(IDataRecord dataRecord, int i)
            => new ColumnValue(new ColumnInfo(dataRecord.GetName(i), dataRecord.GetFieldType(i), dataRecord.GetDataTypeName(i), i));

        private static BufferWriter GetWriter(ColumnCacheKey cacheKey, QueryType queryType)
        {
            QueryIndexer indexer = GetIndexer(cacheKey.Schema);
            BufferParser parser = new BufferParser(cacheKey.Schema, indexer, queryType);
            BufferTree tree = parser.Parse(cacheKey.Items);

            QueryCompiler compiler = new QueryCompiler();

            return compiler.Compile(tree);
        }
    }
}
