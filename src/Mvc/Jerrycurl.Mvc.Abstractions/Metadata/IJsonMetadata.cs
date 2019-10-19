using Jerrycurl.Relations.Metadata;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Mvc.Metadata
{
    public interface IJsonMetadata : IMetadata, IEquatable<IJsonMetadata>
    {
        string Path { get; }
        bool IsRoot { get; }
        IJsonMetadata MemberOf { get; }
        IRelationMetadata Relation { get; }
    }
}
