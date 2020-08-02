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
            };
        }

        private NodeBinder GetReader(Node node, Dictionary<MetadataIdentity, ColumnIdentity> valueMap)
        {
            if (valueMap.TryGetValue(node.Identity, out ColumnIdentity column))
            {
                return new DataBinder()
                {
                    Metadata = node.Metadata,
                    Column = column,
                    CanBeDbNull = true,
                };
            }
            else
            {
                NewBinder reader = new NewBinder()
                {
                    Metadata = node.Metadata,
                    Properties = node.Properties.Select(n => this.GetReader(n, valueMap)).ToList(),
                };

                this.AddPrimaryKey(reader);

                return reader;
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

        private Dictionary<MetadataIdentity, ColumnIdentity> GetValueMap(ListIdentity identity)
            => identity.Columns.ToDictionary(c => new MetadataIdentity(this.Schema, c.Name));
    }
}
