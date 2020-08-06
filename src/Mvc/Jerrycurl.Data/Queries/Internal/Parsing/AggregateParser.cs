using System;
using System.Collections.Generic;
using System.Linq;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Data.Queries.Internal.Binding;
using Jerrycurl.Data.Queries.Internal.Caching;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Data.Queries.Internal.Parsing
{
    internal class AggregateParser
    {
        public ISchema Schema => this.Buffer.Schema;
        public BufferCache Buffer { get; }

        public AggregateParser(BufferCache cache)
        {
            this.Buffer = cache ?? throw new ArgumentException(nameof(cache));
        }

        public AggregateTree Parse(IEnumerable<AggregateName> values)
        {
            NodeTree nodeTree = NodeParser.Parse(this.Schema, values);

            Node itemNode = nodeTree.Items.FirstOrDefault(n => n.Depth == 1);

            return new AggregateTree()
            {
                Schema = this.Schema,
                Aggregate = this.GetBinder(itemNode, values),
            };
        }

        private AggregateBinder FindValue(Node node, IEnumerable<AggregateName> names)
        {
            foreach (AggregateName name in names)
            {
                MetadataIdentity metadata = new MetadataIdentity(node.Metadata.Identity.Schema, name.Name);

                if (metadata.Equals(node.Identity))
                {
                    return new AggregateBinder()
                    {
                        Metadata = node.Metadata,
                        BufferIndex = name.IsPrincipal ? this.Buffer.GetListIndex(metadata) : this.Buffer.GetAggregateIndex(node.Identity),
                        CanBeDbNull = true,
                        IsPrincipal = name.IsPrincipal,
                    };
                }
            }

            return null;
        }

        private NodeBinder GetBinder(Node node, IEnumerable<AggregateName> names)
        {
            AggregateBinder binder = this.FindValue(node, names);

            if (binder != null)
                return binder;

            NewBinder newBinder = new NewBinder()
            {
                Metadata = node.Metadata,
                Properties = node.Properties.Select(n => this.GetBinder(n, names)).ToList(),
                IsDynamic = node.IsDynamic,
            };

            BindingHelper.AddPrimaryKey(newBinder);

            return newBinder;
        }
    }
}
