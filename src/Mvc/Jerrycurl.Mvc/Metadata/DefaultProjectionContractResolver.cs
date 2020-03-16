using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Data.Metadata.Annotations;
using Jerrycurl.Mvc.Metadata.Annotations;

namespace Jerrycurl.Mvc.Metadata
{
    public class DefaultProjectionContractResolver : IProjectionContractResolver
    {
        public ProjectionMetadataFlags GetFlags(IProjectionMetadata metadata)
        {
            IdAttribute id = metadata.Relation.Annotations?.OfType<IdAttribute>().FirstOrDefault();
            OutAttribute out0 = metadata.Relation.Annotations?.OfType<OutAttribute>().FirstOrDefault();
            InAttribute in0 = metadata.Relation.Annotations?.OfType<InAttribute>().FirstOrDefault();

            IReferenceMetadata reference = metadata.Identity.GetMetadata<IReferenceMetadata>();
            ProjectionMetadataFlags flags = ProjectionMetadataFlags.None;

            if (id != null)
                flags |= ProjectionMetadataFlags.Identity;

            if (in0 != null || out0 != null)
            {
                flags |= in0 != null ? ProjectionMetadataFlags.Input : ProjectionMetadataFlags.None;
                flags |= out0 != null ? ProjectionMetadataFlags.Output : ProjectionMetadataFlags.None;
            }
            else if (id != null)
                flags |= ProjectionMetadataFlags.Output;
            else if (reference != null && reference.HasAnyFlag(ReferenceMetadataFlags.Key))
                flags |= ProjectionMetadataFlags.Input | ProjectionMetadataFlags.Output;
            else
                flags |= ProjectionMetadataFlags.Input;

            return flags;
        }
    }
}
