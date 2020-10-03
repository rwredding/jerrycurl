using System.Collections.Generic;
using System.Data;

namespace Jerrycurl.Relations.V11
{
    public interface IRelation3 : IField2
    {
        RelationHeader Header { get; }
        IField2 Source { get; }
        IRelationReader GetReader();
        IDataReader GetDataReader();

        IEnumerable<ITuple> Body { get; }
    }
}
