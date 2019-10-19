using System;
using System.Linq;
using System.Linq.Expressions;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Mvc.Metadata;
using Jerrycurl.Mvc.Projections;

namespace Jerrycurl.Mvc.Sql
{
    public static class FilterExtensions
    {
        /// <summary>
        /// Filters the current projection to include only input attributes.
        /// </summary>
        /// <param name="projection">The current projection</param>
        /// <returns>A new projection containing the filtered attributes.</returns>
        public static IProjection In(this IProjection projection) => projection.With(attributes: projection.Attributes.Where(a => a.Metadata.HasFlag(ProjectionMetadataFlags.Input)));

        /// <summary>
        /// Filters the selected projection to include only input attributes.
        /// </summary>
        /// <param name="projection">The current projection</param>
        /// <param name="expression">Expression selecting a projection target.</param>
        /// <returns>A new projection containing the filtered attributes.</returns>
        public static IProjection In<TModel, TProperty>(this IProjection<TModel> projection, Expression<Func<TModel, TProperty>> expression) => projection.For(expression).In();

        /// <summary>
        /// Filters the current projection to include only output attributes.
        /// </summary>
        /// <param name="projection">The current projection</param>
        /// <returns>A new projection containing the filtered attributes.</returns>
        public static IProjection Out(this IProjection projection) => projection.With(attributes: projection.Attributes.Where(a => a.Metadata.HasFlag(ProjectionMetadataFlags.Output)));

        /// <summary>
        /// Filters the selected projection to include only output attributes.
        /// </summary>
        /// <param name="projection">The current projection</param>
        /// <param name="expression">Expression selecting a projection target.</param>
        /// <returns>A new projection containing the filtered attributes.</returns>
        public static IProjection Out<TModel, TProperty>(this IProjection<TModel> projection, Expression<Func<TModel, TProperty>> expression) => projection.For(expression).Out();

        /// <summary>
        /// Returns the identity attribute of the current projection.
        /// </summary>
        /// <param name="projection">The current projection</param>
        /// <returns>The identity attribute, or <see langword="null"/> if none are found.</returns>
        public static IProjectionAttribute Id(this IProjection projection) => projection.Attributes.FirstOrDefault(a => a.Metadata.HasFlag(ProjectionMetadataFlags.Identity));

        /// <summary>
        /// Returns the identity attribute of the selected projection.
        /// </summary>
        /// <param name="projection">The current projection</param>
        /// <param name="expression">Expression selecting a projection target.</param>
        /// <returns>The identity attribute, or <see langword="null"/> if none are found.</returns>
        public static IProjectionAttribute Id<TModel, TProperty>(this IProjection<TModel> projection, Expression<Func<TModel, TProperty>> expression) => projection.For(expression).Id();

        /// <summary>
        /// Filters the current projection to include only primary key attributes.
        /// </summary>
        /// <param name="projection">The current projection</param>
        /// <returns>A new projection containing the filtered attributes.</returns>
        public static IProjection Key(this IProjection projection) => projection.With(attributes: projection.Attributes.Where(a => a.Metadata.HasFlag(ReferenceMetadataFlags.CandidateKey)));

        /// <summary>
        /// Filters the selected projection to include only primary key attributes.
        /// </summary>
        /// <param name="projection">The current projection</param>
        /// <param name="expression">Expression selecting a projection target.</param>
        /// <returns>A new projection containing the filtered attributes.</returns>
        public static IProjection Key<TModel, TProperty>(this IProjection<TModel> projection, Expression<Func<TModel, TProperty>> expression) => projection.For(expression).Key();

        /// <summary>
        /// Filters the current projection to include only primary key attributes.
        /// </summary>
        /// <param name="projection">The current projection</param>
        /// <returns>A new projection containing the filtered attributes.</returns>
        public static IProjection NonKey(this IProjection projection) => projection.With(attributes: projection.Attributes.Where(a => !a.Metadata.HasAnyFlag(ReferenceMetadataFlags.Key)));

        /// <summary>
        /// Filters the selected projection to include only non key attributes.
        /// </summary>
        /// <param name="projection">The current projection</param>
        /// <param name="expression">Expression selecting a projection target.</param>
        /// <returns>A new projection containing the filtered attributes.</returns>
        public static IProjection NonKey<TModel, TProperty>(this IProjection<TModel> projection, Expression<Func<TModel, TProperty>> expression) => projection.For(expression).NonKey();

        /// <summary>
        /// Determines whether or not the current projection contains any attributes.
        /// </summary>
        /// <param name="projection">The current projection</param>
        /// <returns><see langword="true"/> if the projection contains attributes; otherwise <see langword="false"/>.</returns>
        public static bool Any(this IProjection projection) => projection.Attributes.Any();
    }
}
