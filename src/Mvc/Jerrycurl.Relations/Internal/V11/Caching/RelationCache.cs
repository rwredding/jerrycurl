using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Jerrycurl.Relations.Internal.Nodes;
using Jerrycurl.Relations.Internal.V11.Compilation;
using Jerrycurl.Relations.Internal.V11.IO;
using Jerrycurl.Relations.Internal.V11.Parsing;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Relations.Internal.V11.Caching
{
    internal static class RelationCache
    {
        private readonly static ConcurrentDictionary<RelationCacheKey, BufferWriter[]> cache = new ConcurrentDictionary<RelationCacheKey, BufferWriter[]>();

        public static BufferWriter[] GetWriters(MetadataIdentity source, RelationIdentity2 relation)
        {
            RelationCacheKey key = new RelationCacheKey(source, relation);

            return cache.GetOrAdd(key, _ =>
            {
                BufferParser parser = new BufferParser();
                BufferTree tree = parser.Parse(source, relation);
                RelationCompiler compiler = new RelationCompiler();

                return compiler.Compile(tree);
            });
        }
    }
}
