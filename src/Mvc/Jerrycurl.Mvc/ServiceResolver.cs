using Jerrycurl.Mvc.Metadata;
using Jerrycurl.Mvc.Projections;
using Jerrycurl.Relations.Metadata;
using System;
using System.Collections.Generic;

namespace Jerrycurl.Mvc
{
    public class ServiceResolver : IServiceResolver
    {
        public virtual IProjection<TModel> GetProjection<TModel>(IProcContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            ISchema schema = context.Domain.Schemas.GetSchema(typeof(IList<TModel>));
            IProjectionMetadata metadata = schema.GetMetadata<IProjectionMetadata>();

            if (metadata == null)
                throw new ProjectionException("Unable to locate projection metadata for model.");

            ProjectionIdentity identity = new ProjectionIdentity(schema);

            return new Projection<TModel>(identity, context, metadata.Item ?? metadata);
        }

        public virtual TService GetService<TService>()
            where TService : class
        {
            throw new NotSupportedException("Dependency injection is not supported by default. Please implement a custom DI container from the IServiceResolver contract.");
        }
    }
}