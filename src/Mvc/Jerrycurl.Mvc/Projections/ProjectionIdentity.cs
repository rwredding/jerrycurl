using Jerrycurl.Relations;
using Jerrycurl.Relations.Metadata;
using System;

namespace Jerrycurl.Mvc.Projections
{
    public class ProjectionIdentity : IProjectionIdentity
    {
        public IField Field { get; }
        public ISchema Schema { get; }

        public ProjectionIdentity(ISchema schema)
            : this(schema, null)
        {

        }

        public ProjectionIdentity(ISchema schema, IField field)
        {
            this.Schema = schema ?? throw new ArgumentNullException(nameof(schema));
            this.Field = field;

            ProjectionValidator.ValidateIdentity(this);
        }

        public virtual bool Equals(IProjectionIdentity other) => base.Equals(other);
    }
}
