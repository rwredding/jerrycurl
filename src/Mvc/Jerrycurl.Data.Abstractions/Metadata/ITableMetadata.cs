using System;
using System.Collections.Generic;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Data.Metadata
{
    public interface ITableMetadata : IMetadata, IEquatable<ITableMetadata>
    {
        TableMetadataFlags Flags { get; }
        ITableMetadata MemberOf { get; }
        IReadOnlyList<ITableMetadata> Properties { get; }
        ITableMetadata Item { get; }
        IRelationMetadata Relation { get; }

        IReadOnlyList<string> TableName { get; }
        string ColumnName { get; }
    }
}
