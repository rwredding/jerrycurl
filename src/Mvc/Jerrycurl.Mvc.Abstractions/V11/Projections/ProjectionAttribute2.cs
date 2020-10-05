using System;
using System.Collections.Generic;
using System.Text;
using Jerrycurl.Mvc.Metadata;
using Jerrycurl.Relations.V11;

namespace Jerrycurl.Mvc.V11.Projections
{
    public class ProjectionAttribute2 : RelationAttribute
    {
        public IProjectionMetadata Value { get; }
        public new IProjectionMetadata Metadata { get; }

        public ProjectionAttribute2(IProjectionMetadata metadata, IProjectionMetadata value)
            : base(metadata?.Relation)
        {
            this.Metadata = metadata;
            this.Value = value ?? metadata;
        }
    }
}
