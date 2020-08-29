using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Jerrycurl.Relations.Internal.V11.Enumerators;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Relations.Internal.V11.IO
{
    internal class QueueIndex
    {
        public int Buffer { get; set; }
        public IRelationMetadata List { get; set; }
        public IRelationMetadata Item { get; set; }
        public ParameterExpression Variable { get; set; }
        public RelationQueueType Type { get; set; }
    }
}
