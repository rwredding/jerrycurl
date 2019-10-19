using Jerrycurl.Diagnostics;
using Jerrycurl.Relations.Metadata;
using Jerrycurl.Relations.Internal.Nodes;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jerrycurl.Relations.Internal
{
    internal static class FuncCache
    {
        private readonly static ConcurrentDictionary<FuncKey, FuncDescriptor> cache = new ConcurrentDictionary<FuncKey, FuncDescriptor>();

        public static FuncDescriptor GetDescriptor(RelationIdentity relation, MetadataIdentity source)
        {
            FuncKey key = new FuncKey(relation, source);

            return cache.GetOrAdd(key, _ =>
            {
                ListBuilder listBuilder = new ListBuilder(relation, source);
                RelationNode relationNode = listBuilder.Build();
                FuncBuilder funcBuilder = new FuncBuilder(relationNode);

                return funcBuilder.Build();
            });
        }
    }
}
