using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using Jerrycurl.Data.Queries.Internal.V11.Factories;
using Jerrycurl.Data.Queries.Internal.V11.Parsers;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Data.Queries.Internal.V11
{
    internal class QueryCache<TItem>
    {
        private static readonly ConcurrentDictionary<ListIdentity, EnumerateReader<TItem>> enumerateReaders = new ConcurrentDictionary<ListIdentity, EnumerateReader<TItem>>();
        private static readonly ConcurrentDictionary<ListIdentity, BufferWriter> listWriters = new ConcurrentDictionary<ListIdentity, BufferWriter>();
        private static readonly ConcurrentDictionary<ListIdentity, BufferWriter> aggregrateWriters = new ConcurrentDictionary<ListIdentity, BufferWriter>();
        private static readonly ConcurrentDictionary<AggregateIdentity, AggregateReader<TItem>> aggregateReaders = new ConcurrentDictionary<AggregateIdentity, AggregateReader<TItem>>();
        private static readonly ConcurrentDictionary<ISchema, QueryIndexer> indexers = new ConcurrentDictionary<ISchema, QueryIndexer>();

        private static QueryIndexer GetIndexer(ISchema schema) => indexers.GetOrAdd(schema, new QueryIndexer());

        public static AggregateReader<TItem> GetAggregateReader(AggregateIdentity identity)
        {
            return aggregateReaders.GetOrAdd(identity, _ =>
            {
                QueryIndexer indexer = GetIndexer(identity.Schema);
                AggregateParser parser = new AggregateParser(identity.Schema, indexer);
                AggregateTree tree = parser.Parse(identity);

                QueryCompiler compiler = new QueryCompiler();

                return compiler.Compile<TItem>(tree);
            });
        }

        public static EnumerateReader<TItem> GetEnumerateReader(ISchemaStore schemas, IDataRecord dataRecord)
        {
            ISchema schema = schemas.GetSchema(typeof(IList<TItem>));
            ListIdentity identity = ListIdentity.FromRecord(schema, dataRecord);

            return enumerateReaders.GetOrAdd(identity, _ =>
            {
                EnumerateParser parser = new EnumerateParser(schema);
                EnumerateTree tree = parser.Parse(identity);

                QueryCompiler compiler = new QueryCompiler();

                return compiler.Compile<TItem>(tree);
            });
        }

        public static BufferWriter GetAggregateWriter(ISchema schema, IDataRecord dataRecord)
        {
            ListIdentity identity = ListIdentity.FromRecord(schema, dataRecord);

            return aggregrateWriters.GetOrAdd(identity, _ => GetWriter(identity, QueryType.Aggregate));
        }

        public static BufferWriter GetListWriter(ISchema schema, IDataRecord dataRecord)
        {
            ListIdentity identity = ListIdentity.FromRecord(schema, dataRecord);

            return listWriters.GetOrAdd(identity, _ => GetWriter(identity, QueryType.List));
        }

        private static BufferWriter GetWriter(ListIdentity identity, QueryType queryType)
        {
            QueryIndexer indexer = GetIndexer(identity.Schema);
            BufferParser parser = new BufferParser(identity.Schema, indexer, queryType);
            BufferTree tree = parser.Parse(identity);

            QueryCompiler compiler = new QueryCompiler();

            return compiler.Compile(tree);
        }
    }
}
