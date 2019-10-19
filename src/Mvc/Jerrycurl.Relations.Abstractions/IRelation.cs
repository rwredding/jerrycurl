using Jerrycurl.Relations.Metadata;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Relations
{
    public interface IRelation : IEnumerable<ITuple>, IField
    {
        new RelationIdentity Identity { get; }
        IField Source { get; }
    }
}
