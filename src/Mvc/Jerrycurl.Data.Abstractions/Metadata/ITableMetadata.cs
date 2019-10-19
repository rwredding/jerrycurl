using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Data.Metadata
{
    public interface ITableMetadata : IMetadata, IEquatable<ITableMetadata>
    {
        TableMetadataFlags Flags { get; }
        ITableMetadata MemberOf { get; }
        IReadOnlyList<ITableMetadata> Properties { get; }
        ITableMetadata Item { get; }

        IReadOnlyList<string> TableName { get; }
        string ColumnName { get; }
    }
}
