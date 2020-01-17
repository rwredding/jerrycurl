using System.Collections.Generic;

namespace Jerrycurl.Relations
{
    public interface IRelation : IEnumerable<ITuple>, IField
    {
        new RelationIdentity Identity { get; }
        IField Source { get; }
    }
}
