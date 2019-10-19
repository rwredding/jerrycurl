using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Relations.Metadata
{
    public interface IRelationContractResolver
    {
        IRelationListContract GetListContract(IRelationMetadata metadata);
        IEnumerable<Attribute> GetAnnotationContract(IRelationMetadata metadata);
    }
}
