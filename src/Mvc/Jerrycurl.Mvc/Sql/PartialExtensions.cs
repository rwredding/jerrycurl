using System;
using System.Linq.Expressions;
using Jerrycurl.Mvc.Metadata;
using Jerrycurl.Mvc.Projections;
using Jerrycurl.Relations;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Mvc.Sql
{
    public static class PartialExtensions
    {
        public static ISqlContent Subquery<TModel, TProperty>(this IProjection<TModel> projection, Expression<Func<TModel, TProperty>> expression, string queryName, object model = null)
            => projection.For(expression).Subquery(queryName, model);

        public static ISqlContent Subquery(this IProjection projection, string queryName, object model = null)
        {
            ISchema modelSchema = projection.Context.Domain.Schemas.GetSchema(model?.GetType() ?? typeof(object));
            IProjectionMetadata modelMetadata = modelSchema.GetMetadata<IProjectionMetadata>();
            IField field = new Relation(model, modelSchema);

            IProjectionIdentity modelIdentity = new ProjectionIdentity(modelSchema, field);

            IProjection modelProjection = new Projection(modelIdentity, projection.Context, modelMetadata);
            IProjection resultProjection = projection;

            return projection.Context.Renderer.Partial(queryName, modelProjection, resultProjection);
        }

        public static ISqlContent Subcommand<TModel, TProperty>(this IProjection<TModel> projection, Expression<Func<TModel, TProperty>> expression, string commandName)
            => projection.For(expression).Subcommand(commandName);

        public static ISqlContent Subcommand(this IProjection projection, string commandName)
        {
            ISchema resultSchema = projection.Context.Domain.Schemas.GetSchema(typeof(object));
            IProjectionMetadata resultMetadata = resultSchema.GetMetadata<IProjectionMetadata>();

            IProjectionIdentity resultIdentity = new ProjectionIdentity(resultSchema);

            IProjection modelProjection = projection;
            IProjection resultProjection = new Projection(resultIdentity, projection.Context, resultMetadata);

            return projection.Context.Renderer.Partial(commandName, modelProjection, resultProjection);
        }
    }
}
