using System;
using System.Collections.Generic;
using System.Linq;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Data.Queries.Internal.Binding;
using Jerrycurl.Data.Queries.Internal.Caching;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Data.Queries.Internal.Parsing
{
    internal class EnumerateParser
    {
        public ISchema Schema { get; }

        public EnumerateParser(ISchema schema)
        {
            this.Schema = schema ?? throw new ArgumentNullException(nameof(schema));
        }

        public EnumerateTree Parse(IEnumerable<ColumnValue> values)
        {
            NodeTree nodeTree = NodeParser.Parse(this.Schema, values);
            Node itemNode = nodeTree.Items.FirstOrDefault(n => n.Depth == 1);

            return new EnumerateTree()
            {
                Schema = this.Schema,
                Item = this.GetReader(itemNode, values),
            };
        }

        private NodeBinder GetReader(Node node, IEnumerable<ColumnValue> values)
        {
            ColumnBinder columnBinder = BindingHelper.FindValue(node, values);

            if (columnBinder != null)
                return columnBinder;

            NewBinder newBinder = new NewBinder()
            {
                Metadata = node.Metadata,
                Properties = node.Properties.Select(n => this.GetReader(n, values)).ToList(),
                IsDynamic = node.IsDynamic,
            };

            BindingHelper.AddPrimaryKey(newBinder);

            return newBinder;
        }
    }
}
