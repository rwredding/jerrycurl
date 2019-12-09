using Jerrycurl.Mvc.Metadata;
using Jerrycurl.Mvc.Projections;
using Jerrycurl.Relations.Metadata;
using System;
using System.Collections.Generic;

namespace Jerrycurl.Mvc
{
    public class ProcServices : IProcServices
    {
        private readonly IServiceProvider serviceProvider;

        public ProcServices(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

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
            if (this.serviceProvider == null)
                throw new NotSupportedException("Dependency injection is not supported by default. Please pass an IServiceProvider to the ProcServices constructor or implement a custom DI container from the IProcServices contract.");

            return (TService)this.serviceProvider.GetService(typeof(TService));
        }
    }
}