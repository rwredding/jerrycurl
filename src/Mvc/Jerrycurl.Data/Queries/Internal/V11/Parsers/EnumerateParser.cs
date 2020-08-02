using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Data.Queries.Internal.V11.Binders;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Data.Queries.Internal.V11.Parsers
{
    internal class EnumerateParser
    {
        public ISchema Schema { get; }

        public EnumerateParser(ISchema schema)
        {
            this.Schema = schema ?? throw new ArgumentNullException(nameof(schema));
        }

        public EnumerateTree Parse(ListIdentity identity)
        {
            NodeParser nodeParser = new NodeParser(this.Schema);
            Dictionary<MetadataIdentity, ColumnIdentity> valueMap = this.GetValueMap(identity);
            NodeTree nodeTree = nodeParser.Parse(valueMap.Keys);

            Node itemNode = nodeTree.Items.FirstOrDefault(n => n.Depth == 1);

            return new EnumerateTree()
            {
                Schema = this.Schema,
                Item = this.GetReader(itemNode, valueMap),
                Helpers = new List<HelperWriter>(),
            };
        }

        private NodeReader GetReader(Node node, Dictionary<MetadataIdentity, ColumnIdentity> valueMap)
        {
            if (valueMap.TryGetValue(node.Identity, out ColumnIdentity column))
            {
                return new DataReader()
                {
                    Metadata = node.Metadata,
                    Column = column,
                    CanBeDbNull = true,
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
                IList<DataReader> keyValues = primaryKey.Properties.Select(m => reader.Properties.FirstOrDefault(p => p.Metadata.Identity.Equals(m.Identity))).OfType<DataReader>().ToList();

                if (keyValues.Count == primaryKey.Properties.Count)
                {
                    reader.PrimaryKey = new KeyReader() { Values = keyValues };

                    foreach (DataReader keyValue in keyValues)
                        keyValue.CanBeDbNull = false;
                }
                    
            }
        }

        private Dictionary<MetadataIdentity, ColumnIdentity> GetValueMap(ListIdentity identity)
            => identity.Columns.ToDictionary(c => new MetadataIdentity(this.Schema, c.Name));
    }
}
