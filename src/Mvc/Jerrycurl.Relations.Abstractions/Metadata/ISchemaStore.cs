using System;
using System.Collections.Generic;

namespace Jerrycurl.Relations.Metadata
{
    public interface ISchemaStore : ICollection<IMetadataBuilder>
    {
        IMetadataNotation Notation { get; }
        ISchema GetSchema(Type modelType);
    }
}
