using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices.ComTypes;
using Jerrycurl.Mvc.Projections;
using Jerrycurl.Relations;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Mvc.Sql
{
    public static class ValueExtensions
    {
        public static IProjectionValues<TModel> Vals<TModel>(this IProjection<TModel> projection)
        {
            if (projection.Source == null)
                return new ProjectionValues<TModel>(projection.Context, projection.Identity, Array.Empty<IProjection<TModel>>());

            IEnumerable<MetadataIdentity> heading = new[] { projection.Attr().Metadata.Identity }.Concat(projection.Attributes.Select(a => a.Metadata.Identity));

            Relation relation = new Relation(projection.Source, heading);
            IProjectionAttribute[] attributes = projection.Attributes.ToArray();

            return new ProjectionValues<TModel>(projection.Context, projection.Identity, InnerVals());

            IEnumerable<IProjection<TModel>> InnerVals()
            {
                foreach (ITuple tuple in relation)
                {
                    IField field = tuple[0];
                    IProjectionAttribute[] newAttributes = attributes.Select((a, i) => a.With(field: () => tuple[i + 1])).ToArray();

                    yield return projection.With(attributes: newAttributes, field: field);
                }
            }
        }

        public static IProjectionValues<TModel> Desc<TModel>(this IProjectionValues<TModel> projections)
            => new ProjectionValues<TModel>(projections.Context, projections.Identity, projections.Items.Reverse());

        public static IProjectionValues<TModel> Union<TModel>(this IProjectionValues<TModel> projections, Expression<Func<TModel, IEnumerable<TModel>>> expression)
        {
            return new ProjectionValues<TModel>(projections.Context, projections.Identity, InnerUnion());

            IEnumerable<IProjection<TModel>> InnerUnion()
            {
                List<IProjection<TModel>> valueList = new List<IProjection<TModel>>();

                foreach (IProjection<TModel> projection in projections)
                {
                    valueList.Add(projection);

                    yield return projection;
                }

                foreach (IProjection<TModel> projection in valueList.SelectMany(p => p.Vals(expression)))
                    yield return projection;
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

        public static IEnumerable<IProjection> Vals(this IProjection projection) => projection.Cast<object>().Vals();
        public static IProjectionValues<TItem> Vals<TModel, TItem>(this IProjection<TModel> projection, Expression<Func<TModel, IEnumerable<TItem>>> expression) => projection.Open(expression).Vals();

        public static IProjection<TProperty> Val<TModel, TProperty>(this IProjection<TModel> projection, Expression<Func<TModel, TProperty>> expression) => projection.For(expression).Val();
        public static IProjection<TModel> Val<TModel>(this IProjection<TModel> projection) => ((IProjection)projection).Val().Cast<TModel>();
    }
}
