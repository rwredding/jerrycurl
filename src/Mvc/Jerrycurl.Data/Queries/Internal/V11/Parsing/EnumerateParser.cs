using System;
using System.Linq;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Data.Queries.Internal.V11.Binding;
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
            NodeTree nodeTree = nodeParser.Parse(identity.Columns.Select(c => new MetadataIdentity(this.Schema, c.Name)));
            Node itemNode = nodeTree.Items.FirstOrDefault(n => n.Depth == 1);

            return new EnumerateTree()
            {
                Schema = this.Schema,
                Item = this.GetReader(itemNode, identity),
            };
        }

        private NodeBinder GetReader(Node node, ListIdentity identity)
        {
            DataBinder dataBinder = NodeHelper.FindData(node, identity);

            if (dataBinder != null)
                return dataBinder;

            NewBinder reader = new NewBinder()
            {
                Metadata = node.Metadata,
                Properties = node.Properties.Select(n => this.GetReader(n, identity)).ToList(),
            };

            this.AddPrimaryKey(reader);

            return reader;
        }

        private void AddPrimaryKey(NewBinder binder)
        {
            IReferenceKey primaryKey = binder.Metadata.Identity.GetMetadata<IReferenceMetadata>()?.Keys.FirstOrDefault(k => k.IsPrimaryKey);
            KeyBinder key = NodeHelper.FindKey(binder, primaryKey);

            if (key != null)
            {
                binder.PrimaryKey = key;

                foreach (ValueBinder value in key.Values)
                    value.CanBeDbNull = false;
            }
        }
    }
}
