using System;
using System.Linq.Expressions;
using Jerrycurl.Data.Commands;
using Jerrycurl.Mvc.Metadata;
using Jerrycurl.Mvc.Projections;
using Jerrycurl.Relations;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Mvc.Sql
{
    public static class PropertyExtensions
    {
        /// <summary>
        /// Appends the current property name, e.g. <c>"Item.MyValue"</c>, to the attribute buffer.
        /// </summary>
        /// <param name="attribute">The current attribute</param>
        /// <param name="tblAlias">An alias to qualify the property name with.</param>
        /// <param name="propName">A name to suffix the property name with.</param>
        /// <returns>A new attribute containing the appended buffer.</returns>
        public static IProjectionAttribute Prop(this IProjectionAttribute attribute, string tblAlias = null, string propName = null)
        {
            if ((propName == null || attribute.Field != null) && attribute.Metadata.HasFlag(RelationMetadataFlags.Model))
                throw ProjectionException.FromProjection(attribute, "Cannot reference model directly.");

            if (attribute.Field != null)
            {
                IField field = attribute.Field();
                
                ColumnBinding binding = new ColumnBinding(field);

                propName = attribute.Context.Domain.Dialect.Identifier(binding.ColumnName);

                return attribute.Append(propName).Append(binding);
            }
            else
            {
                string fullName = attribute.Metadata.Identity.Name;

                if (propName != null)
                    fullName = attribute.Identity.Schema.Notation.Combine(fullName, propName);

                if (tblAlias != null)
                {
                    attribute = attribute.Append(attribute.Context.Domain.Dialect.Identifier(tblAlias));
                    attribute = attribute.Append(attribute.Context.Domain.Dialect.Qualifier);
                }

                return attribute.Append(attribute.Context.Domain.Dialect.Identifier(fullName));
            }
        }

        /// <summary>
        /// Appends the current property names, e.g. <c>"Item.MyValue"</c>, to the projection buffer.
        /// </summary>
        /// <param name="projection">The current attribute</param>
        /// <param name="tblAlias">An alias to qualify the property name with.</param>
        /// <returns>A new projection containing the appended buffer.</returns>
        public static IProjection Props(this IProjection projection, string tblAlias = null) => projection.Map(a => a.Prop(tblAlias));

        /// <summary>
        /// Appends the selected property names, e.g. <c>"Item.MyValue"</c>, to the projection buffer.
        /// </summary>
        /// <param name="projection">The current projection</param>
        /// <param name="expression">Expression selecting a specific attribute.</param>
        /// <param name="tblAlias">An alias to qualify each property name with.</param>
        /// <returns>A new projection containing the appended buffer.</returns>
        public static IProjection Props<TModel, TProperty>(this IProjection<TModel> projection, Expression<Func<TModel, TProperty>> expression, string tblAlias = null) => projection.For(expression).Props(tblAlias);

        /// <summary>
        /// Appends the selected property name, e.g. <c>"Item.MyValue"</c>, to a new attribute buffer.
        /// </summary>
        /// <param name="projection">The current projection</param>
        /// <param name="expression">Expression selecting a specific attribute.</param>
        /// <param name="tblAlias">An alias to qualify the property name with.</param>
        /// <param name="propName">A name to suffix the property name with.</param>
        /// <returns>A new attribute containing the appended buffer.</returns>
        public static IProjectionAttribute Prop<TModel, TProperty>(this IProjection<TModel> projection, Expression<Func<TModel, TProperty>> expression, string tblAlias = null, string propName = null)
            => projection.Attr(expression).Prop(tblAlias, propName);

        /// <summary>
        /// Appends the current property name, e.g. <c>"Item.MyValue"</c>, to a new attribute buffer.
        /// </summary>
        /// <param name="projection">The current projection</param>
        /// <param name="tblAlias">An alias to qualify the property name with.</param>
        /// <param name="propName">A name to suffix the property name with.</param>
        /// <returns>A new attribute containing the appended buffer.</returns>
        public static IProjectionAttribute Prop(this IProjection projection, string tblAlias = null, string propName = null)
            => projection.Attr().Prop(tblAlias, propName);  
    }
}
