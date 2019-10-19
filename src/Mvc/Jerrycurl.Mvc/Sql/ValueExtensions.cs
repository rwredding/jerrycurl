using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Jerrycurl.Mvc.Projections;
using Jerrycurl.Relations;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Mvc.Sql
{
    public static class ValueExtensions
    {
        public static IEnumerable<IProjection> Vals(this IProjection projection)
        {
            if (projection.Source == null)
                yield break;

            IEnumerable<MetadataIdentity> heading = new[] { projection.Attr().Metadata.Identity }.Concat(projection.Attributes.Select(a => a.Metadata.Identity));

            Relation relation = new Relation(projection.Source, heading);
            IProjectionAttribute[] attributes = projection.Attributes.ToArray();

            foreach (ITuple tuple in relation)
            {
                IField field = tuple[0];
                IProjectionAttribute[] newAttributes = attributes.Select((a, i) => a.With(field: () => tuple[i + 1])).ToArray();

                yield return projection.With(attributes: newAttributes, field: field);

                projection.Context.Executing.Buffer.Mark();
            }
        }

        public static IProjection Val(this IProjection projection)
        {
            IProjection value = projection.Vals().FirstOrDefault();

            if (value == null)
                throw ProjectionException.ValueNotFound(projection);

            return value;
        }

        public static IProjectionAttribute ValList(this IProjection projection, Func<IProjectionAttribute, IProjectionAttribute> itemFactory)
        {
            if (projection.Source == null)
                throw ProjectionException.ValueNotFound(projection);

            IField[] items = new Relation(projection.Source, projection.Metadata.Identity.Name).Column().ToArray();
            IProjectionAttribute attribute = projection.Attr();

            if (items.Length == 0)
                return attribute;

            attribute = itemFactory(attribute.With(metadata: attribute.Metadata, field: () => items[0]));

            foreach (IField item in items.Skip(1))
                attribute = itemFactory(attribute.With(field: () => item).Append(", "));

            return attribute;
        }

        public static IEnumerable<IProjection<TModel>> Vals<TModel>(this IProjection<TModel> projection) => ((IProjection)projection).Vals().Select(p => p.Cast<TModel>());
        public static IEnumerable<IProjection<TItem>> Vals<TModel, TItem>(this IProjection<TModel> projection, Expression<Func<TModel, IEnumerable<TItem>>> expression) => projection.Open(expression).Vals();

        public static IProjection<TProperty> Val<TModel, TProperty>(this IProjection<TModel> projection, Expression<Func<TModel, TProperty>> expression) => projection.For(expression).Val();
        public static IProjection<TModel> Val<TModel>(this IProjection<TModel> projection) => ((IProjection)projection).Val().Cast<TModel>();
    }
}
