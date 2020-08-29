using System.Collections.Generic;
using System.Data;

namespace Jerrycurl.Relations.Internal.V11
{
    public interface IRelation3 : IField2
    {
        RelationHeader Header { get; }
        IField2 Source { get; }
        IRelationReader GetReader();
        IDataReader GetDataReader();

        IEnumerable<IField2[]> Body { get; }
    }
}
