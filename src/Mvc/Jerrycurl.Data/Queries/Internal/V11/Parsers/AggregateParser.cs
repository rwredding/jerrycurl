using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jerrycurl.Collections;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Data.Queries.Internal.V11.Binders;
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
            NodeParser nodeParser = new NodeParser(this.Schema);
            HashSet<MetadataIdentity> valueMap = new HashSet<MetadataIdentity>(identity);
            NodeTree nodeTree = nodeParser.Parse(valueMap);

            Node itemNode = nodeTree.Items.FirstOrDefault(n => n.Depth == 1);

            return new AggregateTree()
            {
                Schema = this.Schema,
                Aggregate = this.GetBinder(itemNode, valueMap),
            };
        }

        private NodeBinder GetBinder(Node node, HashSet<MetadataIdentity> valueMap)
        {
            if (valueMap.Contains(node.Identity))
            {
                return new AggregateBinder()
                {
                    Metadata = node.Metadata,
                    CanBeDbNull = true,
                    BufferIndex = this.Indexer.GetAggregateIndex(node.Identity),
                };
            }
            else
            {
                NewBinder binder = new NewBinder()
                {
                    Metadata = node.Metadata,
                    Properties = node.Properties.Select(n => this.GetBinder(n, valueMap)).ToList(),
                };

                this.AddPrimaryKey(binder);

                return binder;
            }
        }

        private void AddPrimaryKey(NewBinder binder)
        {
            IReferenceKey primaryKey = binder.Metadata.Identity.GetMetadata<IReferenceMetadata>()?.Keys.FirstOrDefault(k => k.IsPrimaryKey);
            ValueKey key = KeyHelper.FindKey(binder, primaryKey.Properties);

            if (key != null)
            {
                binder.PrimaryKey = key;

                foreach (ValueBinder value in key.Values)
                    value.CanBeDbNull = false;
            }
        }
    }
}
