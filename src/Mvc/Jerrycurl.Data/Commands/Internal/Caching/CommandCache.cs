using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Jerrycurl.Data.Commands.Internal.Caching;
using Jerrycurl.Data.Commands.Internal.Compilation;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Data.Commands.Internal.Caching
{
    internal static class CommandCache
    {
        private static readonly ConcurrentDictionary<CommandCacheKey, BufferWriter> writerMap = new ConcurrentDictionary<CommandCacheKey, BufferWriter>();
        private static readonly ConcurrentDictionary<CommandCacheKey, BufferWriter> converterMap = new ConcurrentDictionary<CommandCacheKey, BufferWriter>();

        public static BufferWriter GetWriter(IReadOnlyList<ColumnName> columnNames)
        {
            CommandCacheKey cacheKey = new CommandCacheKey(columnNames);

            return writerMap.GetOrAdd(cacheKey, k =>
            {
                CommandCompiler compiler = new CommandCompiler();

                return compiler.Compile(k.Columns);
            });
        }

        public static BufferConverter GetConverter(MetadataIdentity metadata, ColumnMetadata columnMetadata)
        {
            return new CommandCompiler().Compile(metadata, columnMetadata);
        }
    }
}
