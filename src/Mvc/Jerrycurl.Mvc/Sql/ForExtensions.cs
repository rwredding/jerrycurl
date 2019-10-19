using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Jerrycurl.Mvc.Metadata;
using Jerrycurl.Mvc.Projections;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Mvc.Sql
{
    public static class ForExtensions
    {
        /// <summary>
        /// Navigates the current projection to a selected target. 
        /// </summary>
        /// <param name="projection">The current projection.</param>
        /// <param name="expression">Expression selecting a projection target.</param>
        /// <returns>A new projection from the selected target.</returns>
        public static IProjection<TProperty> For<TModel, TProperty>(this IProjection<TModel> projection, Expression<Func<TModel, TProperty>> expression)
        {
            IProjectionMetadata metadata = ProjectionHelper.GetMetadataFromRelativeLambda(projection, expression);

            return projection.With(metadata).Cast<TProperty>();
        }

        /// <summary>
        /// Navigates the current projection to its default item target.
        /// </summary>
        /// <param name="projection">The current projection.</param>
        /// <returns>A new projection of the item target.</returns>
        public static IProjection Open(this IProjection projection)
        {
            if (!projection.Metadata.HasFlag(RelationMetadataFlags.List))
                throw ProjectionException.FromProjection(projection, "No metadata list contract found.");

            return projection.With(metadata: projection.Metadata.Item);
        }

        /// <summary>
        /// Casts the current anonymous projection to a projection of a specified type.
        /// </summary>
        /// <typeparam name="TModel">The type to cast the projection to.</typeparam>
        /// <param name="projection">The current projection.</param>
        /// <returns>A projection of the specified type.</returns>
        public static IProjection<TModel> Cast<TModel>(this IProjection projection) => new Projection<TModel>(projection);

        /// <summary>
        /// Navigates the current projection to its default item target.
        /// </summary>
        /// <param name="projection">The current projection.</param>
        /// <returns>A new, typed projection of the item target.</returns>
        public static IProjection<TItem> Open<TItem>(this IProjection<IEnumerable<TItem>> projection) => ((IProjection)projection).Open().Cast<TItem>();

        /// <summary>
        /// Navigates the current projection to the default item target of a selected target.
        /// </summary>
        /// <param name="projection">The current projection.</param>
        /// <param name="expression">Expression selecting a projection target.</param>
        /// <returns>A new, typed projection of the item target.</returns>
        public static IProjection<TItem> Open<TModel, TItem>(this IProjection<TModel> projection, Expression<Func<TModel, IEnumerable<TItem>>> expression) => projection.For(expression).Open();

        /// <summary>
        /// Gets the attribute of the current projection.
        /// </summary>
        /// <param name="projection">The current projection</param>
        /// <returns>A new attribute</returns>
        public static IProjectionAttribute Attr(this IProjection projection) => new ProjectionAttribute(projection);
        public static IProjectionAttribute Attr<TModel, TProperty>(this IProjection<TModel> projection, Expression<Func<TModel, TProperty>> expression) => projection.For(expression).Attr();

        public static IEnumerable<IProjectionAttribute> Attrs(this IProjection projection) => projection.Attributes;
        public static IEnumerable<IProjectionAttribute> Attrs<TModel, TProperty>(this IProjection<TModel> projection, Expression<Func<TModel, TProperty>> expression) => projection.For(expression).Attrs();

        public static IProjection<TProperty> For<TModel, TItem, TProperty>(this IProjection<TModel> projection, Expression<Func<TModel, IEnumerable<TItem>>> listExpression, Expression<Func<TItem, TProperty>> propertyExpression)
            => projection.Open(listExpression).For(propertyExpression);
    }
}
