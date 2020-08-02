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
                Aggregate = this.GetReader(itemNode, valueMap),
            };
        }

        private NodeReader GetReader(Node node, HashSet<MetadataIdentity> valueMap)
        {
            if (valueMap.Contains(node.Identity))
            {
                return new AggregateReader()
                {
                    Metadata = node.Metadata,
                    CanBeDbNull = true,
                    BufferIndex = this.Indexer.GetAggregateIndex(node.Identity),
                };
            }
            else
            {
                NewReader reader = new NewReader()
                {
                    Metadata = node.Metadata,
                    Properties = node.Properties.Select(n => this.GetReader(n, valueMap)).ToList(),
                };

                this.AddPrimaryKeys(reader);

                return reader;
            }
        }

        private void AddPrimaryKeys(NewReader reader)
        {
            IReferenceMetadata reference = reader.Metadata.Identity.GetMetadata<IReferenceMetadata>();
            IReferenceKey primaryKey = reference.Keys.FirstOrDefault(k => k.IsPrimaryKey);

            if (primaryKey != null)
            {
                IList<AggregateReader> keyValues = primaryKey.Properties.Select(m => reader.Properties.First(p => p.Metadata.Identity.Equals(m.Identity))).OfType<AggregateReader>().ToList();

                if (keyValues.Count == primaryKey.Properties.Count)
                {
                    reader.PrimaryKey = new KeyReader() { Values = keyValues };

                    foreach (AggregateReader keyValue in keyValues)
                        keyValue.CanBeDbNull = false;
                }
            }
        }
    }
}
