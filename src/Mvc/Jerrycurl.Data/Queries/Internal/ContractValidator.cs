using Jerrycurl.Data.Metadata;
using Jerrycurl.Data.Queries.Internal.Nodes;
using Jerrycurl.Reflection;
using System.Linq;
using System.Linq.Expressions;

namespace Jerrycurl.Data.Queries.Internal
{
    internal static class ContractValidator
    {
        public static void Validate(AggregateNode aggregateNode)
        {
            if (aggregateNode.Item == null)
                return;

            foreach (MetadataNode node in aggregateNode.Item.Tree().Skip(1))
            {
                if (!node.Metadata.HasFlag(BindingMetadataFlags.Writable))
                    throw BindingException.FromProperty(node.Identity.Name, $"Property is read-only.");

                if (node.ListIndex == null)
                    ValidateConstructor(node);

                if (node.ListIndex == null && node.HasFlag(NodeFlags.Dynamic))
                    ValidateDynamic(node);
            }
        }

        public static void Validate(ResultNode resultNode)
        {
            foreach (ListNode listNode in resultNode.Lists)
                Validate(listNode);

            foreach (ElementNode elementNode in resultNode.Elements)
                Validate(elementNode.Value);
        }

        public static void Validate(MetadataNode itemNode)
        {
            foreach (MetadataNode node in itemNode.Tree())
            {
                if (!node.Metadata.HasFlag(BindingMetadataFlags.Item) && !node.Metadata.HasFlag(BindingMetadataFlags.Writable))
                    throw BindingException.FromProperty(node.Identity.Name, $"Property is read-only.");

                if (node.Column == null)
                    ValidateConstructor(node);

                if (node.Column == null && node.HasFlag(NodeFlags.Dynamic))
                    ValidateDynamic(node);
            }
        }

        private static void Validate(ListNode listNode)
        {
            if (listNode.Metadata.HasFlag(BindingMetadataFlags.Item) && listNode.Metadata.Parent.Composition?.Add == null)
                throw BindingException.FromMetadata(listNode.Metadata.Parent, $"List add method not found.");
        }

        private static void ValidateDynamic(MetadataNode node)
        {
            if (node.Metadata.Composition?.AddDynamic == null)
                throw BindingException.FromProperty(node.Identity.Name, $"Dynamic add method not found.");
        }

        private static void ValidateConstructor(MetadataNode node)
        {
            NewExpression constructor = node.Metadata.Composition?.Construct;

            if (constructor == null)
                throw BindingException.FromProperty(node.Identity.Name, $"No default constructor defined.");
            else if (!node.Metadata.Type.IsAssignableFrom(constructor.Type))
                throw BindingException.FromProperty(node.Identity.Name, $"No conversion exists between constructor type '{constructor.Type.GetSanitizedName()}' and property type '{node.Metadata.Type.GetSanitizedName()}'.");
        }
    }
}
