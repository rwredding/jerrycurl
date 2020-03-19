using System;

namespace Jerrycurl.Mvc.Metadata
{
    [Flags]
    public enum ProjectionMetadataFlags
    {
        None = 0,
        Input = 1,
        Output = 2,
        Identity = 4,
        OutputParameter = 8,
    }
}
