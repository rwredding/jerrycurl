using Jerrycurl.Data.Metadata;
using Jerrycurl.Mvc.Metadata;
using Jerrycurl.Mvc.Projections;
using Jerrycurl.Relations;
using System;
using System.Linq.Expressions;

namespace Jerrycurl.Mvc.Sql
{
    internal static class ProjectionHelper
    {
        public static ITableMetadata GetPreferredTableMetadata(IProjection projection) => GetPreferredTableMetadata(projection.Metadata) ??
            throw ProjectionException.FromProjection(projection, "No table information found.");

        public static ITableMetadata GetPreferredTableMetadata(IProjectionAttribute attribute) => GetPreferredTableMetadata(attribute.Metadata) ??
            throw ProjectionException.FromProjection(attribute, "No table information found.");

        private static ITableMetadata GetPreferredTableMetadata(IProjectionMetadata metadata)
        {
            ITableMetadata table = metadata.Table;
            ITableMetadata item = metadata.Item?.Table;

            if (table != null && table.HasFlag(TableMetadataFlags.Table))
                return table;
            else if (table?.MemberOf != null && table.MemberOf.HasFlag(TableMetadataFlags.Table))
                return table.MemberOf;
            else if (item != null && item.HasFlag(TableMetadataFlags.Table))
                return item;

            return null;
        }

        public static ITableMetadata GetPreferredColumnMetadata(IProjectionAttribute attribute)
        {
            if (attribute.Metadata.HasFlag(TableMetadataFlags.Column))
                return attribute.Metadata.Table;
            else if (attribute.Metadata.Item != null && attribute.Metadata.Item.HasFlag(TableMetadataFlags.Column))
                return attribute.Metadata.Item.Table;

            throw ProjectionException.FromProjection(attribute, "No column information found.");
        }

        public static IProjectionMetadata GetMetadataFromRelativeLambda(IProjection projection, LambdaExpression expression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            string name = projection.Metadata.Identity.Notation.Lambda(expression);
            string fullName = projection.Metadata.Identity.Notation.Combine(projection.Metadata.Identity.Name, name);

            return projection.Metadata.Identity.Schema.GetMetadata<IProjectionMetadata>(fullName) ?? throw ProjectionException.FromProjection(projection, "Metadata not found.");
        }

        public static IField GetFieldValue(IProjectionAttribute attribute)
        {
            if (attribute.Field == null)
                throw ProjectionException.ValueNotFound(attribute);

            return attribute.Field();
        }
    }
}
