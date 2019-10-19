using System;
using System.Collections.Generic;
using Jerrycurl.Mvc.Metadata;
using Jerrycurl.Relations;

namespace Jerrycurl.Mvc.Projections
{
    public class Projection<TModel> : Projection, IProjection<TModel>
    {
        public Projection(IProjectionIdentity identity, IProcContext context)
            : base(identity, context)
        {

        }

        public Projection(IProjection projection)
            : base(projection)
        {

        }

        internal Projection(IProjectionIdentity identity, IProcContext context, IProjectionMetadata metadata)
            : base(identity, context, metadata)
        {

        }

        public new IProjection<TModel> Map(Func<IProjectionAttribute, IProjectionAttribute> m) => new Projection<TModel>(base.Map(m));

        public new IProjection<TModel> With(IProjectionMetadata metadata = null,
                                            IEnumerable<IProjectionAttribute> attributes = null,
                                            IField field = null,
                                            IProjectionOptions options = null) => new Projection<TModel>(base.With(metadata, attributes, field, options));
    }
}
