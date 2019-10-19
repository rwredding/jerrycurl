using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Jerrycurl.Relations.Metadata
{
    public interface IRelationListContract
    {
        Type ItemType { get; }
        string ItemName { get; }
        MethodInfo WriteIndex { get; }
        MethodInfo ReadIndex { get; }
    }
}
