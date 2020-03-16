using System;
using System.Collections.Generic;

namespace Jerrycurl.Mvc.Metadata
{
    public interface IProjectionContractResolver
    {
        ProjectionMetadataFlags GetFlags(IProjectionMetadata metadata);
    }
}
