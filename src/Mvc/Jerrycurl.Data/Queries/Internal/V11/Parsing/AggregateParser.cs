using System;
using System.Collections.Generic;
using System.Linq;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Data.Queries.Internal.V11.Binding;
using Jerrycurl.Data.Queries.Internal.V11.Caching;
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

        public AggregateTree Parse(IEnumerable<AggregateValue> values)
        {
            NodeTree nodeTree = NodeParser.Parse(this.Schema, values);

            Node itemNode = nodeTree.Items.FirstOrDefault(n => n.Depth == 1);

            return new AggregateTree()
            {
                Schema = this.Schema,
                Aggregate = this.GetBinder(itemNode, values),
            };
        }

        private AggregateBinder FindValue(Node node, IEnumerable<AggregateValue> values)
        {
            foreach (AggregateValue value in values)
            {
                MetadataIdentity metadata = new MetadataIdentity(node.Metadata.Identity.Schema, value.Name);

                if (metadata.Equals(node.Identity))
                {
                    return new AggregateBinder()
                    {
                        Metadata = node.Metadata,
                        BufferIndex = value.IsPrincipal ? this.Indexer.GetListIndex(metadata) : this.Indexer.GetAggregateIndex(node.Identity),
                        CanBeDbNull = true,
                        IsPrincipal = value.IsPrincipal,
                    };
                }
            }

            return null;
        }

        private NodeBinder GetBinder(Node node, IEnumerable<AggregateValue> valueSpan)
        {
            AggregateBinder binder = this.FindValue(node, valueSpan);

            if (binder != null)
                return binder;

            NewBinder newBinder = new NewBinder()
            {
                Metadata = node.Metadata,
                Properties = node.Properties.Select(n => this.GetBinder(n, valueSpan)).ToList(),
            };

            BindingHelper.AddPrimaryKey(newBinder);

            return newBinder;
        }
    }
}
