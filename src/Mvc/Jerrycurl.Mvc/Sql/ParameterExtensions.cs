using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Jerrycurl.Data;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Data.Commands;
using Jerrycurl.Mvc.Metadata;
using Jerrycurl.Mvc.Projections;
using Jerrycurl.Relations;
using Jerrycurl.Relations.Metadata;
using Jerrycurl.Collections;

namespace Jerrycurl.Mvc.Sql
{
    public static class ParameterExtensions
    {
        public static IProjection IsEq(this IProjection projection, IProjection other)
        {
            List<IProjectionAttribute> newAttrs = new List<IProjectionAttribute>();

            foreach (var (l, r) in projection.Attrs().Zip(other.Attrs()))
            {
                IProjectionAttribute newAttr = l;

                newAttr = newAttr.Eq();
                newAttr = newAttr.With(metadata: r.Metadata, field: r.Field).Par();
                newAttr = newAttr.With(metadata: l.Metadata, field: l.Field);

                newAttrs.Add(newAttr);
            }

            return projection.With(attributes: newAttrs);
        }

        /// <summary>
        /// Appends the current parameter name and value, e.g. <c>@P0</c>, to the attribute buffer.
        /// </summary>
        /// <param name="attribute">The current attribute.</param>
        /// <returns>A new attribute containing the appended buffer.</returns>
        public static IProjectionAttribute Par(this IProjectionAttribute attribute)
        {
            IField value = attribute.Field?.Invoke();

            if (value != null)
            {
                string paramName = attribute.Context.Lookup.Parameter(attribute.Identity, value);
                string dialectName = attribute.Context.Domain.Dialect.Parameter(paramName);

                Parameter param = new Parameter(paramName, value);

                IProjectionAttribute result = attribute.Append(dialectName).Append(param);

                if (attribute.Metadata.HasFlag(ProjectionMetadataFlags.Output))
                {
                    ParameterBinding binding = new ParameterBinding(param.Name, value);

                    result = result.Append(binding);
                }

                return result;
            }
            else
            {
                string paramName = attribute.Context.Lookup.Parameter(attribute.Identity, attribute.Metadata.Identity);
                string dialectName = attribute.Context.Domain.Dialect.Parameter(paramName);

                return attribute.Append(dialectName);
            }
        }

        /// <summary>
        /// Appends the current parameter name and value, e.g. <c>@P0</c>, to a new attribute buffer.
        /// </summary>
        /// <param name="projection">The current projection.</param>
        /// <returns>A new attribute containing the appended buffer.</returns>
        public static IProjectionAttribute Par(this IProjection projection) => projection.Attr().Par();

        /// <summary>
        /// Appends the selected parameter name and value, e.g. <c>@P0</c>, to a new attribute buffer.
        /// </summary>
        /// <param name="projection">The current projection.</param>
        /// <param name="expression">Expression selecting a projection target.</param>
        /// <returns>A new attribute containing the appended buffer.</returns>
        public static IProjectionAttribute Par<TModel, TProperty>(this IProjection<TModel> projection, Expression<Func<TModel, TProperty>> expression)
            => projection.Attr(expression).Par();

        /// <summary>
        /// Appends the current parameter names and values, e.g. <c>@P0</c>, to the projection buffer.
        /// </summary>
        /// <param name="projection">The current projection.</param>
        /// <returns>A new projection containing the appended buffer.</returns>
        public static IProjection Pars(this IProjection projection) => projection.Map(a => a.Par());

        /// <summary>
        /// Appends the selected parameter names and values, e.g. <c>@P0</c>, to the projection buffer.
        /// </summary>
        /// <param name="projection">The current projection.</param>
        /// <param name="expression">Expression selecting a projection target.</param>
        /// <returns>A new projection containing the appended buffer.</returns>
        public static IProjection Pars<TModel, TProperty>(this IProjection<TModel> projection, Expression<Func<TModel, TProperty>> expression)
            => projection.For(expression).Pars();

        /// <summary>
        /// Appends a comma-separated list of parameter names and values, e.g. <c>@P0, @P1, @P2</c>, to a new attribute buffer.
        /// </summary>
        /// <param name="projection">The current projection.</param>
        /// <returns>A new attribute containing the appended buffer.</returns>
        public static IProjectionAttribute ParList(this IProjection projection) => projection.ValList(a => a.Par());

        /// <summary>
        /// Appends a comma-separated list of parameter names and values, e.g. <c>@P0, @P1, @P2</c>, to a new attribute buffer.
        /// </summary>
        /// <param name="projection">The current projection.</param>
        /// /// <param name="expression">Expression selecting a projection target.</param>
        /// <returns>A new attribute containing the appended buffer.</returns>
        public static IProjectionAttribute ParList<TModel, TItem>(this IProjection<TModel> projection, Expression<Func<TModel, IEnumerable<TItem>>> expression)
            => projection.Open(expression).ParList();
    }
}
