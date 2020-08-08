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
                Aggregate = itemNode != null ? this.CreateBinder(itemNode, values) : this.GetEmptyBinder(),
            };
        }

        private NewBinder GetEmptyBinder()
        {
            IBindingMetadata metadata = this.Schema.GetMetadata<IBindingMetadata>().Item;

            return new NewBinder(metadata);
        }

        private AggregateBinder FindValue(Node node, IEnumerable<AggregateName> names)
        {
            foreach (AggregateName name in names)
            {
                MetadataIdentity metadata = new MetadataIdentity(node.Metadata.Identity.Schema, name.Name);

                if (metadata.Equals(node.Identity))
                {
                    AggregateBinder value = new AggregateBinder(node)
                    {
                        CanBeDbNull = true,
                        IsPrincipal = name.IsPrincipal,
                    };

                    if (value.IsPrincipal)
                        value.BufferIndex = this.Buffer.GetListIndex(metadata);
                    else
                        value.BufferIndex = this.Buffer.GetAggregateIndex(metadata);

                    return value;
                }
            }

            return null;
        }

        private NodeBinder CreateBinder(Node node, IEnumerable<AggregateName> valueNames)
        {
            AggregateBinder value = this.FindValue(node, valueNames);

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
