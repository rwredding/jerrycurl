using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

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
