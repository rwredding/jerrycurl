using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Relations.Metadata
{
    public interface IRelationContractContext
    {
        IRelationMetadata Metadata { get; }
    }
}
