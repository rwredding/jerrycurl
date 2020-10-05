using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Jerrycurl.Diagnostics;
using Jerrycurl.Relations.Internal.Nodes;
using Jerrycurl.Relations.V11.Internal.Compilation;
using Jerrycurl.Relations.V11.Internal.Enumerators;
using Jerrycurl.Relations.V11.Internal.IO;
using Jerrycurl.Relations.V11.Internal.Parsing;
using Jerrycurl.Relations.Metadata;
using HashCode = Jerrycurl.Diagnostics.HashCode;

namespace Jerrycurl.Relations.V11.Internal.Caching
{
    internal static class RelationCache
    {
        private readonly static ConcurrentDictionary<RelationCacheKey, BufferWriter> cache = new ConcurrentDictionary<RelationCacheKey, BufferWriter>();

        public static RelationBuffer CreateBuffer2(IRelation2 relation)
        {
            BufferWriter writer = GetWriter(relation.Source.Identity.Metadata, relation.Header);

            return new RelationBuffer()
            {
                Writer = writer,
                Queues = new IRelationQueue[writer.Queues.Length],
                Fields = new IField2[relation.Header.Attributes.Count],
                Model = relation.Source.Model,
                Source = relation.Source,
            };
        }

        private static BufferWriter GetWriter(MetadataIdentity source, RelationHeader header)
        {
            RelationCacheKey key = new RelationCacheKey(source, header);

            return cache.GetOrAdd(key, _ =>
            {
                BufferParser parser = new BufferParser();
                BufferTree tree = parser.Parse(source, header);
                RelationCompiler compiler = new RelationCompiler();

                return compiler.Compile(tree);
            });
        }
    }
}
