using System.Collections.Generic;
using System.Data;

namespace Jerrycurl.Relations.V11
{
    public interface IRelation2
    {
        RelationHeader Header { get; }
        IField2 Source { get; }
        IRelationReader GetReader();
        IDataReader GetDataReader();

        IEnumerable<ITuple2> Body { get; }
    }
}
