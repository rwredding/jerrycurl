using System;
using System.Reflection;

namespace Jerrycurl.Relations.Metadata
{
    public class RelationListContract : IRelationListContract
    {
        public Type ItemType { get; set; }
        public string ItemName { get; set; } = "Item";
        public MethodInfo WriteIndex { get; set; }
        public MethodInfo ReadIndex { get; set; }
    }
}
