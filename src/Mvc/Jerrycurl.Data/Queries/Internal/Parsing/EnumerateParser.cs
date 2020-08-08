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

        public EnumerateTree Parse(IEnumerable<ColumnName> values)
        {
            NodeTree nodeTree = NodeParser.Parse(this.Schema, values);
            Node itemNode = nodeTree.Items.FirstOrDefault(n => n.Depth == 1);

            return new EnumerateTree()
            {
                Schema = this.Schema,
                Item = this.CreateBinder(itemNode, values),
            };
        }

        private NodeBinder CreateBinder(Node node, IEnumerable<ColumnName> valueNames)
        {
            ColumnBinder value = BindingHelper.FindValue(node, valueNames);

            if (value != null)
                return value;

            NewBinder binder = new NewBinder(node)
            {
                Properties = node.Properties.Select(n => this.CreateBinder(n, valueNames)).ToList(),
            };

            BindingHelper.AddPrimaryKey(binder);

            return binder;
        }
    }
}
