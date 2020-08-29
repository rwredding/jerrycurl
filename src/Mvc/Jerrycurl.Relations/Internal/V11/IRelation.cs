using System.Collections.Generic;
using System.Data;

namespace Jerrycurl.Relations.Internal.V11
{
    public interface IRelation2 : IField2
    {
        new RelationIdentity2 Identity { get; }
        IField2 Source { get; }
        IRelationReader GetReader();
        IDataReader GetDataReader();

        IEnumerable<IField2[]> Body { get; }
    }
}
