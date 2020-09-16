using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Data.Queries.Internal.Compilation;
using Jerrycurl.Data.Queries.Internal.Parsing;
using Jerrycurl.Relations.Metadata;
using AggregateCacheKey = Jerrycurl.Data.Queries.Internal.Caching.QueryCacheKey<Jerrycurl.Data.Queries.Internal.Caching.AggregateName>;
using ColumnCacheKey = Jerrycurl.Data.Queries.Internal.Caching.QueryCacheKey<Jerrycurl.Data.Queries.Internal.Caching.ColumnName>;

namespace Jerrycurl.Data.Queries.Internal.Caching
{
    internal class QueryCache<TItem>
    {
        private static readonly ConcurrentDictionary<ColumnCacheKey, EnumerateReader<TItem>> enumerateReaders = new ConcurrentDictionary<ColumnCacheKey, EnumerateReader<TItem>>();
        private static readonly ConcurrentDictionary<ColumnCacheKey, BufferWriter> listWriters = new ConcurrentDictionary<ColumnCacheKey, BufferWriter>();
        private static readonly ConcurrentDictionary<ColumnCacheKey, BufferWriter> aggregrateWriters = new ConcurrentDictionary<ColumnCacheKey, BufferWriter>();
        private static readonly ConcurrentDictionary<AggregateCacheKey, AggregateReader<TItem>> aggregateReaders = new ConcurrentDictionary<AggregateCacheKey, AggregateReader<TItem>>();
        private static readonly ConcurrentDictionary<ISchema, BufferCache> buffers = new ConcurrentDictionary<ISchema, BufferCache>();

        private static BufferCache GetBuffer(ISchema schema) => buffers.GetOrAdd(schema, s => new BufferCache(s));

        public static AggregateReader<TItem> GetAggregateReader(AggregateCacheKey cacheKey)
        {
            return aggregateReaders.GetOrAdd(cacheKey, k =>
            {
                BufferCache buffer = GetBuffer(k.Schema);
                AggregateParser parser = new AggregateParser(buffer);
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
            IEnumerable<ColumnName> columns = Enumerable.Range(0, GetFieldCount()).Select(i => GetColumnName(dataRecord, i));

            return new ColumnCacheKey(schema, columns);

            int GetFieldCount()
            {
                try { return dataRecord.FieldCount; }
                catch { return 0; }
            }
        }

        public static ColumnName GetColumnName(IDataRecord dataRecord, int i)
            => new ColumnName(new ColumnMetadata(dataRecord.GetName(i), dataRecord.GetFieldType(i), dataRecord.GetDataTypeName(i), i));

        private static BufferWriter GetWriter(ColumnCacheKey cacheKey, QueryType queryType)
        {
            BufferCache buffer = GetBuffer(cacheKey.Schema);
            BufferParser parser = new BufferParser(queryType, buffer);
            BufferTree tree = parser.Parse(cacheKey.Items);

            QueryCompiler compiler = new QueryCompiler();

            return compiler.Compile(tree);
        }
    }
}
